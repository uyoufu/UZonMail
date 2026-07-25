namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 新增或更新邮件模板时允许客户端提交的数据。
    /// </summary>
    public class UpsertEmailTemplateDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
