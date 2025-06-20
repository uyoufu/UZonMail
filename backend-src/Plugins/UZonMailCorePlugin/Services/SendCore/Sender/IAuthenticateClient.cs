using MailKit.Net.Proxy;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.SendCore.Sender
{
    public interface IAuthenticateClient
    {
        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task AuthenticateAsync(string email, string username, string password);

        /// <summary>
        /// 授权验证测试
        /// 对于 OAuth2 的邮箱验证，前面 3 个参数没有意义
        /// </summary>
        /// <param name="smtpHost"></param>
        /// <param name="smtpPort"></param>
        /// <param name="enableSSL"></param>
        /// <param name="email"></param>
        /// <param name="smtpUserName"></param>
        /// <param name="smtpPassword"></param>
        /// <param name="proxyClient"></param>
        /// <returns></returns>
        Task<Result<string>> AuthenticateTestAsync(string email, string smtpUserName, string smtpPassword, IProxyClient? proxyClient = null,
            string smtpHost = "", int smtpPort = 465, bool enableSSL = true);
    }
}
