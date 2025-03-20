using log4net;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Collections.Concurrent;
using Uamazing.Utils.Envs;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender
{
    public class SmtpClientFactory(ProxyManager proxyManager) : ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmtpClientFactory));
        private readonly ConcurrentDictionary<string, ThrottlingSmtpClient> _smptClients = new();

        /// <summary>
        /// 获取 smtp 客户端
        /// 有可能更换了账号密码，要重新获取
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public async Task<Result<SmtpClient>> GetSmtpClientAsync(SendingContext sendingContext)
        {
            var outbox = sendingContext.EmailItem!.Outbox;
            var key = outbox.Email;
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

            _logger.Info($"初始化 SmtpClient: {outbox.AuthUserName}");
            //var userInfo = await DBCacher.GetCache<UserInfoCache>(sendingContext.SqlContext, outbox.UserId.ToString());
            //var orgSetting = await DBCacher.GetCache<OrganizationSettingCache>(sendingContext.SqlContext, userInfo.OrganizationId);
            //int cooldownMilliseconds = orgSetting?.Setting.MinOutboxCooldownSecond ?? 0;
            //var client = new ThrottlingSmtpClient(outbox.Email, cooldownMilliseconds);

            // [TODO] 此处应该不需要限制频率，因为发件箱处已经处理了，后期测试后再决定是否添加
            var client = new ThrottlingSmtpClient(outbox.Email, 0);
            try
            {
                // 获取代理
                var proxyHandler = await proxyManager.GetProxyHandler(sendingContext);
                if (proxyHandler != null)
                    client.ProxyClient = await proxyHandler.GetProxyClientAsync(key);

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
            if(client.ProxyClient == null)
            {
                return true;
            }

            // 判断是否到达了更换代理的次数
            var sentTotal = sendingContext.EmailItem!.Outbox.SentTotal;
            if (sentTotal == 0) return true;
            if (sentTotal % orgSetting.ChangeIpAfterEmailCount != 0) return true;

            return false;
        }
    }
}
