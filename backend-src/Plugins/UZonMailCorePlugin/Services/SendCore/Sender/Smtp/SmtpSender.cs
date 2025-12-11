using log4net;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UZonMail.CorePlugin.Services.Encrypt;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Proxies;
using UZonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Results;

namespace UZonMail.CorePlugin.Services.SendCore.Sender.Smtp
{
    /// <summary>
    /// 本机邮件发送器
    /// </summary>
    public class SmtpSender(
        IServiceProvider provider,
        EncryptService encryptService,
        ProxiesManager proxyManager,
        IPRateLimiter iPRateLimiter
    ) : IEmailSender
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmtpSender));

        public int Order => int.MaxValue;

        public bool IsMatch(string email, string smtpHost)
        {
            return true;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<IHandlerResult> SendAsync(SendingContext context, MimeMessage message)
        {
            return await SendAsync(context, message, 3);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        private async Task<IHandlerResult> SendAsync(
            SendingContext context,
            MimeMessage message,
            int tryCount
        )
        {
            SendItemMeta sendItem = context.EmailItem!;

            // 获取 smtp 客户端
            var smtpClientManager = context.Provider.GetRequiredService<SmtpClientsManager>();
            var clientResult = await smtpClientManager.GetSmtpClientAsync(context);

            //#if DEBUG
            //            // 模拟获取 smtp 客户端失败的情况
            //            clientResult.Ok = false;
            //#endif

            // 若返回 null,说明这个发件箱不能建立 smtp 连接，对它进行取消
            if (!clientResult)
            {
                var errorMessage = $"发件箱 {sendItem.Outbox.Email} 错误。{clientResult.Message}";
                _logger.Error(errorMessage);
                // 标记发件箱有问题
                context.OutboxAddress?.MarkShouldDispose(errorMessage);
                return HandlerResult.Failed(errorMessage);
            }
            // throw new NullReferenceException("测试报错");
            var smtpClient = clientResult.Data;
            // 等待发送间隔
            // 等待之后，可能出现代理过期的问题
            await iPRateLimiter.WaitForReleaseAsync(
                context,
                sendItem.Outbox.Email,
                smtpClient.ProxyClient?.ProxyHost
            );
            // 如果是动态代理，则需要检测动态代理是否过期
            if (smtpClient.ProxyClient is ProxyClientAdapter proxyAdapter && !proxyAdapter.IsEnable)
            {
                // 动态代理不可用，重新执行发送逻辑
                return await SendAsync(context, message, tryCount - 1);
            }
            try
            {
                var sendResult = await smtpClient.SendAsync(message);
                _logger.Info(
                    $"邮件发送完成：{sendItem.Outbox.Email} -> {string.Join(",", sendItem.Inboxes.Select(x => x.Email))}"
                );

                // 标记邮件状态
                sendItem.SetStatus(SendItemMetaStatus.Success, sendResult);
                return HandlerResult.Success(sendResult);
            }
            // 错误情况分类
            // 1. 代理刚好失效，导致发生 ServiceNotConnectedException ，从而导致失败
            // 2. 发件箱有问题
            catch (SmtpCommandException smtpCommandException)
            {
                // 收件箱不可达
                if (smtpCommandException.ErrorCode == SmtpErrorCode.RecipientNotAccepted)
                {
                    _logger.Warn(smtpCommandException);
                    sendItem.SetStatus(SendItemMetaStatus.Error, smtpCommandException.Message);
                    return HandlerResult.Failed(smtpCommandException.Message);
                }
                else
                {
                    // 发件箱有问题
                    sendItem.Outbox?.MarkShouldDispose(smtpCommandException.Message);
                    return HandlerResult.Failed(smtpCommandException.Message);
                }
            }
            catch (ServiceNotConnectedException ex)
            {
                // 已经重试多次，说明发件箱有问题
                if (tryCount < 0)
                {
                    _logger.Error(ex);
                    // 发件箱问题，返回失败
                    sendItem.Outbox?.MarkShouldDispose(ex.Message);
                    return HandlerResult.Failed(ex.Message);
                }

                // 代理无法连接到服务器
                // 在发件前已经验证过邮件可用，此处应当作是代理出了问题
                // 将当前代理标记为不可用
                if (smtpClient.ProxyClient is ProxyClientAdapter clientAdapter)
                {
                    clientAdapter.MarkHealthless();
                }

                return await SendAsync(context, message, tryCount - 1);
            }
            catch (Exception error)
            {
                _logger.Error(error);
                // 发件箱问题，返回失败
                sendItem.Outbox?.MarkShouldDispose(error.Message);
                return HandlerResult.Failed(error.Message);
            }
        }

        /// <summary>
        /// 验证发件箱
        /// </summary>
        /// <param name="db"></param>
        /// <param name="outbox">密码应是加密后的字符串</param>
        /// <returns></returns>
        public async Task<Result<string>> TestOutbox(
            IServiceProvider scopeServiceProvider,
            Outbox outbox
        )
        {
            var client = provider.GetRequiredService<ThrottlingSmtpClient>();
            client.SetParams(string.Empty, 0);

            var email = outbox.Email;
            var smtpHost = outbox.SmtpHost;
            var smtpPort = outbox.SmtpPort;
            var smtpUserName = string.IsNullOrEmpty(outbox.UserName)
                ? outbox.Email
                : outbox.UserName;
            var smtpPassword = encryptService.DecryptPassword(outbox.Password);
            var enableSSL = outbox.EnableSSL;

            // 判断参数是否正确
            if (string.IsNullOrEmpty(smtpHost))
            {
                return Result<string>.Fail("SMTP 服务器地址不能为空");
            }
            if (smtpPort <= 0 || smtpPort > 65535)
            {
                return Result<string>.Fail("SMTP 端口号不正确");
            }
            if (string.IsNullOrEmpty(smtpUserName))
            {
                return Result<string>.Fail("SMTP 用户名不能为空");
            }
            if (string.IsNullOrEmpty(smtpPassword))
            {
                return Result<string>.Fail("SMTP 密码不能为空");
            }

            // 获取代理客户端
            // 参考：https://github.com/jstedfast/MailKit/tree/master/Documentation/Examples
            if (outbox.ProxyId > 0)
            {
                var proxyHandler = await proxyManager.GetProxyHandler(
                    scopeServiceProvider,
                    outbox.UserId,
                    email,
                    outbox.ProxyId
                );
                if (proxyHandler != null)
                    client.ProxyClient = await proxyHandler.GetProxyClientAsync(
                        scopeServiceProvider,
                        email
                    );
            }

            string sendResult = $"{smtpUserName} test success";
            try
            {
                await client.ConnectAsync(smtpHost, smtpPort, enableSSL);
                // 鉴权
                if (!string.IsNullOrEmpty(smtpPassword))
                {
                    await client.AuthenticateAsync(email, smtpUserName, smtpPassword);
                }
                return Result<string>.Success(sendResult);
            }
            // 证书过期不再回退
            //catch (SslHandshakeException ex)
            //{
            //    _logger.Warn(ex);
            //    // 可能是证书过期
            //    await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
            //    // 鉴权
            //    if (!string.IsNullOrEmpty(smtpPassword))
            //    {
            //        await client.AuthenticateAsync(email, smtpUserName, smtpPassword);
            //    }
            //    return Result<string>.Success(sendResult);
            //}
            catch (Exception ex)
            {
                _logger.Warn(ex);
                return Result<string>.Fail(ex.Message);
            }
            finally
            {
                // 断开连接
                await client.DisconnectAsync(true);
            }
        }
    }
}
