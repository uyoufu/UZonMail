using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.MailConversations;

namespace UzonMail.DB.SQL.Core.Todos;

/// <summary>
/// 待办分支创建时选择的原会话邮件，用于永久追溯来源。
/// </summary>
public sealed class TodoMailBranchSourceMessage
    : SqlId,
        IEntityTypeConfiguration<TodoMailBranchSourceMessage>
{
    public long TodoMailBranchId { get; set; }
    public TodoMailBranch TodoMailBranch { get; set; } = null!;
    public long MailConversationMessageId { get; set; }
    public MailConversationMessage MailConversationMessage { get; set; } = null!;

    public void Configure(EntityTypeBuilder<TodoMailBranchSourceMessage> builder)
    {
        builder.ToTable("TodoMailBranchSourceMessages");
        builder.HasIndex(x => new { x.TodoMailBranchId, x.MailConversationMessageId }).IsUnique();
        builder
            .HasOne(x => x.TodoMailBranch)
            .WithMany(x => x.SourceMessages)
            .HasForeignKey(x => x.TodoMailBranchId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.MailConversationMessage)
            .WithMany()
            .HasForeignKey(x => x.MailConversationMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
