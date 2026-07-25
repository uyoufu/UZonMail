using Newtonsoft.Json.Linq;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 发件请求中的邮箱地址。
    /// </summary>
    public class EmailAddressDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    /// <summary>
    /// 立即发送邮件时允许客户端提交的数据。
    /// </summary>
    public class SendEmailNowDto
    {
        public string Subjects { get; set; } = string.Empty;
        public List<long> TemplateIds { get; set; } = [];
        public string? Body { get; set; }
        public List<long> OutboxIds { get; set; } = [];
        public List<long> OutboxGroupIds { get; set; } = [];
        public List<EmailAddressDto> Inboxes { get; set; } = [];
        public List<long> InboxGroupIds { get; set; } = [];
        public List<EmailAddressDto> CcBoxes { get; set; } = [];
        public List<EmailAddressDto> BccBoxes { get; set; } = [];
        public List<long> AttachmentIds { get; set; } = [];
        public JArray? Data { get; set; }
        public bool SendBatch { get; set; }
        public List<long> ProxyIds { get; set; } = [];
    }

    /// <summary>
    /// 定时发送邮件时允许客户端提交的数据。
    /// </summary>
    public class ScheduleEmailDto : SendEmailNowDto
    {
        public DateTime ScheduleDate { get; set; }
    }
}
