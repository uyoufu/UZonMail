using MimeKit;
using UZonMail.Core.Services.Encrypt;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.SendCore.Sender
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
        /// 开发发送邮件
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        Task SendAsync(SendingContext sendingContext, MimeMessage mimeMessage);

        /// <summary>
        /// 获取验证客户端
        /// </summary>
        /// <returns></returns>
        Task<Result<string>> TestOutbox(IServiceProvider scopeServiceProvider, Outbox outbox);
    }
}
