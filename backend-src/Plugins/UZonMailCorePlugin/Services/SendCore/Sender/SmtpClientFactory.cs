using log4net;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Collections.Concurrent;
using System.Timers;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.DB.Managers.Cache;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;
using Timer = System.Timers.Timer;

namespace UZonMail.Core.Services.SendCore.Sender
{
    /// <summary>
    /// 按发件任务缓存 SmtpClient, 因此发件完成后，要手动进行释放
    /// </summary>
    public class SmtpClientFactory : ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmtpClientFactory));
        private readonly ConcurrentDictionary<string, ThrottlingSmtpClient> _smptClients = new();


        private readonly ProxyManager _proxyManager;
        private readonly GroupTasksList _groupTaskList;
        private readonly Timer _timer;

        public SmtpClientFactory(ProxyManager proxyManager, GroupTasksList groupTaskList)
        {
            _proxyManager = proxyManager;
            _groupTaskList = groupTaskList;

            // 新建定时器，定时清理过期的 SmtpClient
            _timer = new Timer(1000 * 60 * 5);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            DisposeSmtpClients();
        }

        /// <summary>
        /// 获取 smtp 客户端
        /// 有可能更换了账号密码，要重新获取
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public async Task<Result<SmtpClient>> GetSmtpClientAsync(SendingContext sendingContext)
        {
            var outbox = sendingContext.EmailItem!.Outbox;
            var key = $"{sendingContext.EmailItem.SendingItem.SendingGroupId}:{outbox.Email}";
            if (_smptClients.TryGetValue(key, out var existClient))
            {
                // 验证存活
                var clientAlive = await VerifyClientAlive(existClient, sendingContext);
                if (clientAlive)
                {
                    return new Result<SmtpClient>() { Data = existClient };
                }

                // 说明已经断开,进行移除
                _smptClients.TryRemove(key, out _);
                // 不等待，后台断开
                existClient.DisconnectAsync(true);
            }

            _logger.Debug($"初始化 SmtpClient: {outbox.AuthUserName}");

            // [TODO] 此处应该不需要限制频率，因为发件箱处已经处理了，后期测试后再决定是否添加
            var client = new ThrottlingSmtpClient(outbox.Email, 0);
            try
            {
                // 获取代理
                client.ProxyClient = await GetProxyClient(sendingContext);

                // 对证书过期进行兼容处理
                try
                {
                    client.Connect(outbox.SmtpHost, outbox.SmtpPort, outbox.EnableSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);
                }
                catch (SslHandshakeException ex)
                {
                    _logger.Warn(ex);
                    // 证书过期
                    client.Connect(outbox.SmtpHost, outbox.SmtpPort, SecureSocketOptions.None);
                }

                // Note: only needed if the SMTP server requires authentication
                // 进行鉴权
                var debugConfig = sendingContext.Provider.GetRequiredService<DebugConfig>();
                if (!debugConfig.IsDemo)
                    if (!string.IsNullOrEmpty(outbox.AuthPassword)) client.Authenticate(outbox.AuthUserName, outbox.AuthPassword);

                _smptClients.TryAdd(key, client);
                return new Result<SmtpClient>() { Data = client };
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                client.Disconnect(true);
                client.Dispose();
                return new Result<SmtpClient>()
                {
                    Ok = false,
                    Message = ex.Message,
                    Data = null,
                };
            }
        }

        /// <summary>
        /// 验证 smtp client 是否存活
        /// 1. 已经断开连接
        /// 2. 需要更换代理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private static async Task<bool> VerifyClientAlive(ThrottlingSmtpClient client, SendingContext sendingContext)
        {
            if (!client.IsConnected)
            {
                _logger.Warn($"SmtpClient: {sendingContext.EmailItem!.Outbox.Email} 已经断开连接");
                return false;
            }

            // 判断发件项是否指定了代理
            // 指定代理，不需要更换
            var proxyId = sendingContext.EmailItem!.ProxyId;
            if (proxyId > 0)
            {
                return true;
            }

            // 未设置代理更换时，不更换代理
            var orgSetting = await CacheManager.Global.GetCache<OrganizationSettingCache>(sendingContext.SqlContext, sendingContext.EmailItem.UserId);
            if (orgSetting.ChangeIpAfterEmailCount <= 0)
            {
                return true;
            }

            // 当代理为空时，也不再更换
            // 若某次没获取到代理，将永远失去代理功能，因此在获取代理时，要进行容错处理
            // 有可能存在情况：第一次不使用代理，第二次任务使用代理
            if (client.ProxyClient == null)
            {
                return true;
            }

            // 判断是否到达了更换代理的次数
            var sentTotal = sendingContext.EmailItem!.Outbox.SentTotal;
            if (sentTotal == 0) return true;
            if (sentTotal % orgSetting.ChangeIpAfterEmailCount != 0) return true;

            return false;
        }


        /// <summary>
        /// 获取代理客户端
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        private async Task<IProxyClient?> GetProxyClient(SendingContext sendingContext, int tryCount = 3)
        {
            var outbox = sendingContext.EmailItem!.Outbox;
            if (tryCount < 0)
            {
                _logger.Warn($"邮箱 {outbox.Email} 获取代理失败，尝试次数已达上限");
                return null;
            }

            // 获取代理
            var proxyHandler = await _proxyManager.GetProxyHandler(sendingContext);
            if (proxyHandler == null)
            {
                _logger.Warn($"未能为 {outbox} 匹配到代理, 1 秒后重试...");
                // 1 秒后重试
                await Task.Delay(1000);
                return await GetProxyClient(sendingContext, tryCount - 1);
            }


            var proxyClient = await proxyHandler.GetProxyClientAsync(sendingContext.Provider, outbox.Email);
            if (proxyClient == null)
            {
                _logger.Warn($"从代理 {proxyHandler.Id} 生成代理失败, 1 秒后重试...");
                // 1 秒后重试
                await Task.Delay(1000);
                return await GetProxyClient(sendingContext, tryCount - 1);
            }

            _logger.Debug($"{outbox.Email} 开始使用代理 {proxyHandler.Id}");
            return proxyClient;
        }

        /// <summary>
        /// 手动释放发件任务的 smtp client
        /// </summary>
        public void DisposeSmtpClients()
        {
            var keys = _smptClients.Keys.ToList();
            if (keys.Count == 0) return;

            // 获取任务组
            var taskGroupIds = keys.Select(x => x.Split(":")[0]).Distinct().Select(x=> long.Parse(x)).ToList();
            foreach(var taskGroupId in taskGroupIds)
            {
                // 判断是否存在任务组
                if(_groupTaskList.ContainsKey(taskGroupId))
                {
                    continue;
                }

                var pgPrefix = $"{taskGroupId}:";
                var removingKeys = _smptClients.Keys.Where(x => x.StartsWith(pgPrefix)).ToList();
                foreach (var key in removingKeys)
                {
                    if (_smptClients.TryRemove(key, out var client))
                    {
                        client.Disconnect(true);
                        client.Dispose();
                    }
                }
            }
        }
    }
}
