using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Todos;

/// <summary>
/// 当前用户需要持续跟进的普通事项或邮件事项。
/// </summary>
public sealed class TodoTask : UserAndOrgId, IEntityTypeConfiguration<TodoTask>
{
    public TodoTaskKind Kind { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TodoTaskStatus Status { get; set; }
    public TodoTaskPriority Priority { get; set; } = TodoTaskPriority.Normal;
    public DateTime? DueAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public TodoMailBranch? MailBranch { get; set; }

    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.ToTable("TodoTasks");
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(10000);
        builder.HasIndex(x => new
        {
            x.UserId,
            x.Status,
            x.DueAtUtc,
            x.Id
        });
        builder.HasIndex(x => new
        {
            x.UserId,
            x.UpdatedAtUtc,
            x.Id
        });
    }
}
