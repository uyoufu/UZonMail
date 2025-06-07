using MailKit.Net.Smtp;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender.Authentication
{
    public interface ISmtpAuthenticate : ISingletonService<ISmtpAuthenticate>
    {
        /// <summary>
        /// 序号
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        bool Match(string outbox);

        Task AuthenticateAsync(SmtpClient smtpClient, string email, string username, string password);
    }
}
