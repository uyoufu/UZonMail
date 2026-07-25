using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 创建邮箱组时允许客户端提交的数据。
    /// </summary>
    public class CreateEmailGroupDto
    {
        public EmailGroupType Type { get; set; }
        public string? Icon { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long Order { get; set; }
    }

    /// <summary>
    /// 更新邮箱组时允许客户端修改的数据。
    /// </summary>
    public class UpdateEmailGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long Order { get; set; }
    }
}
