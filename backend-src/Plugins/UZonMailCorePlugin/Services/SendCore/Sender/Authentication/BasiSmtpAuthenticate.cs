using MailKit.Net.Smtp;
using UZonMail.Core.Services.SendCore.Outboxes;

namespace UZonMail.Core.Services.SendCore.Sender.Authentication
{
    /// <summary>
    /// 基础的验证，使用账号密码进行验证
    /// </summary>
    public class BasiSmtpAuthenticate : ISmtpAuthenticate
    {
        public int Order => 9999;

        public async Task AuthenticateAsync(SmtpClient smtpClient, string email, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                // 没有用户名密码，直接返回
                return;
            }

            await smtpClient.AuthenticateAsync(username, password);
        }

        /// <summary>
        /// 最后兜底，所有的都适配
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public bool Match(string outbox)
        {
            return true;
        }
    }
}
