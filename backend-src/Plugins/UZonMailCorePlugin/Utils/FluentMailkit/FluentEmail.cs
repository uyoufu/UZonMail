using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UZonMail.CorePlugin.Services.Emails;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.DB.SQL.Core.Emails;

namespace UZonMail.CorePlugin.Utils.FluentMailkit
{
    /// <summary>
    /// SmtpClient 的 fluent 版本
    /// </summary>
    public class FluentEmail(SmtpClient? smtpClient = null, MimeMessage? mime = null)
    {
        private readonly SmtpClient _smtpClient = smtpClient ?? new SmtpClient();
        private readonly MimeMessage _mimeMessage = mime ?? new MimeMessage();
        private readonly BodyBuilder _bodyBuilder = new();

        public FluentEmail WithFrom(string email, string? userName = "")
        {
            _mimeMessage.From.Add(new MailboxAddress(userName, email));
            return this;
        }

        public FluentEmail WithTo(string email, string? userName = "")
        {
            _mimeMessage.To.Add(new MailboxAddress(userName, email));
            return this;
        }

        public FluentEmail WithCC(string email, string? userName = "")
        {
            _mimeMessage.Cc.Add(new MailboxAddress(userName, email));
            return this;
        }

        public FluentEmail WithBCC(string email, string? userName = "")
        {
            _mimeMessage.Bcc.Add(new MailboxAddress(userName, email));
            return this;
        }

        public FluentEmail WithSubject(string subject)
        {
            _mimeMessage.Subject = subject;
            return this;
        }

        public FluentEmail WithReplyTo(string email, string? userName = "")
        {
            _mimeMessage.ReplyTo.Add(new MailboxAddress(userName, email));
            return this;
        }

        public FluentEmail WithHtmlBody(string htmlBody)
        {
            _bodyBuilder.HtmlBody = htmlBody;
            return this;
        }

        /// <summary>
        /// 添加附件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public FluentEmail WithAttachment(string filePath, string fileName = "")
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileName(filePath);
            }

            _bodyBuilder.Attachments.Add(filePath);
            var lastOne = _bodyBuilder.Attachments.Last();
            lastOne.ContentType.Name = fileName;
            lastOne.ContentDisposition.FileName = fileName;
            return this;
        }

        public FluentEmail WithProxy(IProxyClient? proxyClient)
        {
            if (proxyClient == null)
                return this;

            _smtpClient.ProxyClient = proxyClient;
            return this;
        }

        public FluentEmail ConnectTo(string smptHost, int smtpPort, bool useSsl)
        {
            _smtpClient.Connect(smptHost, smtpPort, useSsl);
            return this;
        }

        public FluentEmail Authenticate(string userName, string password)
        {
            _smtpClient.Authenticate(userName, password);
            return this;
        }

        /// <summary>
        /// 发送邮箱
        /// </summary>
        /// <returns></returns>
        public async Task<string> SendAsync()
        {
            // 设置邮件内容
            _mimeMessage.Body = _bodyBuilder.ToMessageBody();
            // 发送邮件
            return await _smtpClient.SendAsync(_mimeMessage);
        }
    }
}
