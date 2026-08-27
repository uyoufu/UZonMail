using MailKit.Net.Proxy;
using MimeKit;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender
{
    public interface IEmailSendingClient : ITransientService<IEmailSendingClient>
    {
        /// <summary>
        /// 设置发送邮件的参数
        /// </summary>
        /// <param name="email"></param>
        void SetParams(string email, int cooldownMilliseconds);

        /// <summary>
        /// 发送邮箱
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        Task<string> SendAsync(MimeMessage mimeMessage);

        /// <summary>
        /// 代理客户端
        /// </summary>
        IProxyClient? ProxyClient { get; set; }
    }
}
