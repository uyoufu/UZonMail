namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 已发送邮件的完整只读视图，避免向客户端暴露数据库实体和文件存储信息。
    /// </summary>
    public sealed class SendingItemDetailDto
    {
        public long Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public List<EmailAddressDto> Recipients { get; set; } = [];
        public List<EmailAddressDto> CcRecipients { get; set; } = [];
        public List<EmailAddressDto> BccRecipients { get; set; } = [];
        public string Content { get; set; } = string.Empty;
        public List<SendingItemAttachmentDto> Attachments { get; set; } = [];
    }
}
