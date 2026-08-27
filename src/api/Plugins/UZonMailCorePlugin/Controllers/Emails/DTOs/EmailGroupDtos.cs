using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 创建邮箱组时允许客户端提交的数据。排序由服务端维护，避免并发拖拽产生重复顺序。
    /// </summary>
    public class CreateEmailGroupDto
    {
        public EmailGroupCategory Category { get; set; }
        public string? Icon { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// 更新邮箱组时允许客户端修改的数据。
    /// </summary>
    public class UpdateEmailGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// 邮箱分组及其即时聚合的账户数。
    /// </summary>
    public sealed record EmailGroupSummaryDto(
        long Id,
        EmailGroupCategory Category,
        string? Icon,
        string Name,
        string? Description,
        long Order,
        bool IsDefault,
        int AccountCount
    );

    public sealed class ReorderEmailGroupsDto
    {
        public EmailGroupCategory Category { get; set; }
        public List<long> EmailGroupIds { get; set; } = [];
    }
}
