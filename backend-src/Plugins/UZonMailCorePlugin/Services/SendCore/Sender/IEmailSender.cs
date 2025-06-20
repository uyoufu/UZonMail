using MimeKit;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Utils.Web.Service;

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
        /// 是否匹配当前的 outbox
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        bool IsMatch(string outbox);

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
        IAuthenticateClient GetAuthenticateClient();
    }
}
