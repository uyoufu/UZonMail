using MimeKit;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Sender
{
    /// <summary>
    /// 邮件发送器接口
    /// </summary>
    public interface IEmailSender : ISingletonService<IEmailSender>
    {
        /// <summary>
        /// 序号
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 是否匹配当前的 email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        bool IsMatch(string email, string smtpHost);

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        Task<IHandlerResult> SendAsync(SendingContext sendingContext, MimeMessage mimeMessage);

        /// <summary>
        /// 获取验证客户端
        /// </summary>
        /// <returns></returns>
        Task<Result<string>> TestOutbox(IServiceProvider scopeServiceProvider, Outbox outbox);
    }
}
