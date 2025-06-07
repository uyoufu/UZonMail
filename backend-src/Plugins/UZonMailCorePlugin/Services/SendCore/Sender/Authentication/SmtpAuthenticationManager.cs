using MailKit.Net.Smtp;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender.Authentication
{
    /// <summary>
    /// 验证管理器
    /// </summary>
    public class SmtpAuthenticationManager(IEnumerable<ISmtpAuthenticate> authenticators) : ISingletonService
    {
        public async Task AuthenticateAsync(SmtpClient smtpClient, OutboxEmailAddress outbox)
        {
            await AuthenticateAsync(smtpClient, outbox.Email, outbox.AuthUserName, outbox.AuthPassword);
        }

        private List<ISmtpAuthenticate>? _authenticators;
        public async Task AuthenticateAsync(SmtpClient smtpClient, string email, string username, string password)
        {
            _authenticators ??= authenticators.OrderBy(x => x.Order).ToList();

            // 开始获取所有实现
            var authenticator = _authenticators.Where(x => x.Match(email))                
                .FirstOrDefault();
            if (authenticator == null)
                // 说明不需要验证
                return;

            // 开始验证
            await authenticator.AuthenticateAsync(smtpClient, email, username, password);
        }
    }
}
