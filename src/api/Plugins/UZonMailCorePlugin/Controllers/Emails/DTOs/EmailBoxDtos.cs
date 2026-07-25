using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 创建发件箱时允许客户端提交的数据。
    /// </summary>
    public class CreateOutboxDto
    {
        public long EmailGroupId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Remark { get; set; }
        public OutboxType Type { get; set; } = OutboxType.SMTP;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string? UserName { get; set; }
        public string Password { get; set; } = string.Empty;
        public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
        public long ProxyId { get; set; }
        public int MaxSendCountPerDay { get; set; }
        public string? ReplyToEmails { get; set; }
        public int Weight { get; set; }
    }

    /// <summary>
    /// 更新发件箱时允许客户端修改的数据。
    /// </summary>
    public class UpdateOutboxDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public OutboxType Type { get; set; } = OutboxType.SMTP;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string? UserName { get; set; }
        public string Password { get; set; } = string.Empty;
        public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
        public string? Description { get; set; }
        public long ProxyId { get; set; }
        public string? ReplyToEmails { get; set; }
    }

    /// <summary>
    /// 创建收件箱时允许客户端提交的数据。
    /// </summary>
    public class CreateInboxDto
    {
        public long EmailGroupId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Remark { get; set; }
        public long MinInboxCooldownHours { get; set; } = -1;
    }

    /// <summary>
    /// 创建未分组收件箱时允许客户端提交的数据。
    /// </summary>
    public class CreateUngroupedInboxDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Remark { get; set; }
        public long MinInboxCooldownHours { get; set; } = -1;
    }

    /// <summary>
    /// 更新收件箱时允许客户端修改的数据。
    /// </summary>
    public class UpdateInboxDto
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long MinInboxCooldownHours { get; set; } = -1;
    }
}
