using log4net;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Timers;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;
using Timer = System.Timers.Timer;

namespace UZonMail.Core.Services.SendCore.Sender.Smtp
{
    /// <summary>
    /// 按发件任务缓存 SmtpClient, 因此发件完成后，要手动进行释放
    /// </summary>
    public class SmtpClientFactory : ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmtpClientFactory));

        /// <summary>
        /// 发件客户端缓存
        /// 同一个 email，可能同时存在带代理和不带代理的缓存
        /// </summary>
        private readonly ConcurrentDictionary<SmtpClientKey, ThrottlingSmtpClient> _smptClients = new();

        private readonly ProxyManager _proxyManager;
        private readonly GroupTasksList _groupTaskList;
        private readonly AppSettingsManager _settingsService;

        private readonly Timer _timer;

        public SmtpClientFactory(ProxyManager proxyManager, GroupTasksList groupTaskList, AppSettingsManager settingsService)
        {
            _proxyManager = proxyManager;
            _groupTaskList = groupTaskList;
            _settingsService = settingsService;

            // 新建定时器，对 smtp 连接进行保活
            _timer = new Timer(1000 * 30); // 30s
            _timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // 保活
            KeepAlive();
        }

        private void KeepAlive()
        {
            _logger.Info("开始对缓存的 SMTP 连接进行保活");
            Task.Run(async () =>
            {
                var keys = _smptClients.Keys.ToList();
                foreach (var key in keys)
                {
                    if (!_smptClients.TryGetValue(key, out var client)) continue;
                    if (!client.IsConnected)
                    {
                        _smptClients.TryRemove(key, out _);
                        continue;
                    }

                    // 用 if 判断是为了防止并发被释放掉
                    if (client.IsConnected) await client.NoOpAsync();
                }
            });
        }

        /// <summary>
        /// 获取 smtp 客户端
        /// 有可能更换了账号密码，要重新获取
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public async Task<Result<ThrottlingSmtpClient>> GetSmtpClientAsync(SendingContext sendingContext)
        {
            var outbox = sendingContext.EmailItem!.Outbox;

            // 获取缓存客户端是否可用
            var shouldProxy = await ShouldUseProxy(sendingContext);
            var existClient = GetSmtpClientFromCache(outbox.Email, shouldProxy);

            if (existClient != null)
            {
                // 验证存活
                var available = await CheckSmtpClientAvailable(existClient, sendingContext);
                if (available)
                {
                    return new Result<ThrottlingSmtpClient>() { Data = existClient };
                }

                // 说明客户端不可用了，需要移除
                _smptClients.TryRemove(existClient.GetClientKey(), out _);
                // 不等待，后台断开
                existClient.DisconnectAsync(true);
            }

            _logger.Debug($"初始化 SmtpClient: {outbox.AuthUserName}");

            // 获取代理，若代理为空，则不使用代理
            // 当代理失效，但是用户又选择代理时，可能会影响效率，后期进行优化

            // TODO: 此处应该不需要限制频率，因为发件箱处已经处理了，后期测试后再决定是否添加
            var client = sendingContext.Provider.GetRequiredService<ThrottlingSmtpClient>();
            client.SetParams(outbox.Email, 0);
            try
            {
                var result = await SetProxyAndConnectSmtpClient(client, outbox, sendingContext);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                client.Disconnect(true);
                client.Dispose();
                return new Result<ThrottlingSmtpClient>()
                {
                    Ok = false,
                    Message = ex.Message,
                    Data = null,
                };
            }
        }

        /// <summary>
        /// 是否应使用代理
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ShouldUseProxy(SendingContext sendingContext)
        {
            // 判断发件项是否指定了代理
            // 指定代理，不需要更换
            var proxyId = sendingContext.EmailItem!.ProxyId;
            if (proxyId > 0)
            {
                return true;
            }

            // 未设置代理更换时，不更换代理
            var orgSetting = await _settingsService.GetSetting<SendingSetting>(sendingContext.SqlContext, sendingContext.EmailItem.UserId);
            if (orgSetting.ChangeIpAfterEmailCount <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从缓存中获取 smtp client
        /// </summary>
        /// <param name="outbox"></param>
        /// <param name="useProxy"></param>
        /// <returns></returns>
        private ThrottlingSmtpClient? GetSmtpClientFromCache(string email, bool useProxy)
        {
            var key = _smptClients.Keys.Where(x => x.Email == email && !(x.HasProxy ^ useProxy)).FirstOrDefault();
            if (key == null) return null;

            if (!_smptClients.TryGetValue(key, out var value)) return null;
            return value;
        }

        /// <summary>
        /// 验证 smtp client 是否存活
        /// 1. 已经断开连接
        /// 2. 需要更换代理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private async Task<bool> CheckSmtpClientAvailable(ThrottlingSmtpClient client, SendingContext sendingContext)
        {
            if (!client.IsConnected)
            {
                _logger.Warn($"SmtpClient: {sendingContext.EmailItem!.Outbox.Email} 已经断开连接");
                return false;
            }

            // 判断发件项是否指定了代理
            // 若未指定代理，则表示可以正常
            var proxyId = sendingContext.EmailItem!.ProxyId;
            if (proxyId <= 0)
                return true;

            // 判断代理是否失效了
            if (client.ProxyClient is not ProxyClientAdapter proxyClientAdapter)
            {
                _logger.Error($"SmtpClient: {sendingContext.EmailItem!.Outbox.Email} 代理设置异常，赋值 ProxyClient 时请只使用 ProxyClientAdapter 或其子类");
                return false;
            }

            if (!proxyClientAdapter.IsEnable)
                return false;


            // 指定了代理，但是没有设置更换 IP 的次数，返回正常
            // 这种情况，代理可能会失败，要同时考虑
            // 未设置代理更换时，不更换代理
            var orgSetting = await _settingsService.GetSetting<SendingSetting>(sendingContext.SqlContext, sendingContext.EmailItem.UserId);
            if (orgSetting.ChangeIpAfterEmailCount <= 0)
            {
                return true;
            }

            // 判断是否到达了更换代理的次数
            var sentTotal = client.SentCount;
            if (sentTotal == 0) return true;
            if (sentTotal % orgSetting.ChangeIpAfterEmailCount == 0) return false;

            return true;
        }


        /// <summary>
        /// 获取代理客户端，返回时，会对是否可用进行验证
        /// 不能频繁调用，因为内容会验证代理的可用性
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        private async Task<IProxyClient?> GetProxyClient(SendingContext sendingContext, int tryCount = 3)
        {
            var emailItem = sendingContext.EmailItem!;
            var outbox = emailItem.Outbox;
            if (emailItem.AvailableProxyIds.Count == 0)
            {
                _logger.Debug($"邮箱 {outbox.Email} 未配置代理");
                return null;
            }

            if (tryCount < 0)
            {
                _logger.Warn($"邮箱 {outbox.Email} 获取代理失败，尝试次数已达上限");
                return null;
            }

            // 有的 smtpClient 可能不需要代理, 此处要进行判断
            var availableProxyIds = sendingContext.EmailItem!.AvailableProxyIds;
            if (availableProxyIds.Count == 0)
            {
                // 未配置代理，直接返回
                return null;
            }


            // 获取代理，若为空，则重试
            var proxyHandler = await _proxyManager.GetProxyHandler(sendingContext);
            if (proxyHandler == null)
            {
                _logger.Warn($"未能为 {outbox.Email} 匹配到代理, 1 秒后重试...");
                // 1 秒后重试
                await Task.Delay(1000);
                return await GetProxyClient(sendingContext, tryCount - 1);
            }

            // [TODO]: 此处无法完全保证代理可用，因为代理 api 在使用过程中会失效，若检测不及时，则会导致发件失败
            var proxyClient = await proxyHandler.GetProxyClientAsync(sendingContext.Provider, outbox.Email);
            if (proxyClient == null)
            {
                _logger.Warn($"从代理 {proxyHandler.Id} 生成代理失败, 1 秒后重试...");
                // 1 秒后重试
                await Task.Delay(1000);
                return await GetProxyClient(sendingContext, tryCount - 1);
            }

            _logger.Info($"{outbox.Email} 开始使用代理 {proxyHandler.Id}");
            return proxyClient;
        }

        /// <summary>
        /// 连接 smtp client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="outbox"></param>
        /// <param name="sendingContext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<Result<ThrottlingSmtpClient>> SetProxyAndConnectSmtpClient(ThrottlingSmtpClient client, OutboxEmailAddress outbox, SendingContext sendingContext, int tryCount = 3)
        {
            // 获取代理
            var proxyAdapter = await GetProxyClient(sendingContext);
            client.ProxyClient = proxyAdapter;

            // 对证书过期进行兼容处理
            try
            {
                await client.ConnectAsync(outbox.SmtpHost, outbox.SmtpPort, outbox.EnableSSL);
            }
            catch (SocketException ex)
            {
                // 若没有代理，说明是其它错误，不进行尝试
                if (client.ProxyClient == null) throw;

                // 重新获取代理尝试一下
                _logger.Warn($"初始化 {outbox.Email} SmtpClient 时，无法建立 Socket 连接，更换代理尝试，剩余尝试次数: {tryCount}");
                return await SetProxyAndConnectSmtpClient(client, outbox, sendingContext, tryCount - 1);
            }
            catch (SslHandshakeException ex)
            {
                _logger.Warn(ex);
                // 证书过期
                await client.ConnectAsync(outbox.SmtpHost, outbox.SmtpPort, SecureSocketOptions.None);
            }

            // Note: only needed if the SMTP server requires authentication
            // 进行鉴权
            var debugConfig = sendingContext.Provider.GetRequiredService<DebugConfig>();
            if (!debugConfig.IsDemo)
            {
                await client.AuthenticateAsync(outbox.Email, outbox.AuthUserName, outbox.AuthPassword);                
            }
            // 添加到缓存中
            _smptClients.TryAdd(client.GetClientKey(), client);

            return new Result<ThrottlingSmtpClient>() { Data = client };
        }

        /// <summary>
        /// 获取所有的 SmtpClientKeys
        /// </summary>
        public ICollection<SmtpClientKey> SmtpClientKeys => _smptClients.Keys;

        public void DisposeSmtpClient(SmtpClientKey key)
        {
            if (!_smptClients.TryRemove(key, out var client)) return;

            // 进行释放
            if (client.IsConnected)
            {
                client.DisconnectAsync(true);
            }
        }

        public void DisposeSmtpClients(string email)
        {
            _logger.Debug($"移除 SmtpClient {email}");
            var keys = _smptClients.Keys.Where(x => x.Email == email);
            foreach (var key in keys)
            {
                DisposeSmtpClient(key);
            }
        }
    }
}
