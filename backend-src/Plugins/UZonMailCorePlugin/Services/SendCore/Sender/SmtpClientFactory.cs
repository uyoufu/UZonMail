using log4net;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Collections.Concurrent;
using Uamazing.Utils.Envs;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.DB.SQL.Emails;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.SendCore.Sender
{
    public class SmtpClientFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmtpClientFactory));
        private static readonly ConcurrentDictionary<string, SmtpClient> _smptClients = new();

        /// <summary>
        /// 获取 smtp 客户端
        /// 有可能更换了账号密码，要重新获取
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public static async Task<Result<SmtpClient>> GetSmtpClientAsync(SendingContext sendingContext, OutboxEmailAddress outbox, ProxyInfo? proxyInfo)
        {
            var key = outbox.Email;
            if (_smptClients.TryGetValue(key, out var clientValue))
            {
                // 判断是否过期
                if (clientValue.IsConnected)
                {
                    // 更新代理
                    clientValue.ProxyClient = proxyInfo?.GetProxyClient(_logger);
                    return new Result<SmtpClient>() { Data = clientValue };
                }
                // 说明已经断开,进行移除
                await clientValue.DisconnectAsync(true);
            }

            _logger.Info($"初始化 SmtpClient: {outbox.AuthUserName}");
            //var userInfo = await DBCacher.GetCache<UserInfoCache>(sendingContext.SqlContext, outbox.UserId.ToString());
            //var orgSetting = await DBCacher.GetCache<OrganizationSettingCache>(sendingContext.SqlContext, userInfo.OrganizationId);
            //int cooldownMilliseconds = orgSetting?.Setting.MinOutboxCooldownSecond ?? 0;
            //var client = new ThrottlingSmtpClient(outbox.Email, cooldownMilliseconds);

            // [TODO] 此处应该不需要限制频率，发件箱处已经处理了，后期测试后再决定是否添加
            var client = new ThrottlingSmtpClient(outbox.Email, 0);
            try
            {
                if (proxyInfo != null)
                    client.ProxyClient = proxyInfo.GetProxyClient(_logger);

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
                if (!Env.IsDebug)
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
    }
}
