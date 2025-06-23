using MailKit.Net.Proxy;
using MimeKit;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender
{
    public interface IEmailSendingClient : ITransientService<IEmailSendingClient>
    {
        /// <summary>
        /// 设置发送邮件的参数
        /// </summary>
        /// <param name="email"></param>
        void SetParams(string email, int cooldownMilliseconds);

        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task AuthenticateAsync(string email, string username, string password);

        /// <summary>
        /// 发送邮箱
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        Task<string> SendAsync(MimeMessage mimeMessage);

        /// <summary>
        /// 代理客户端
        /// </summary>
        IProxyClient ProxyClient { get; set; }
    }
}
