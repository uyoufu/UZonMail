namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 已发送邮件中可供当前用户查看和下载的附件摘要。
    /// </summary>
    public sealed class SendingItemAttachmentDto
    {
        public long Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
