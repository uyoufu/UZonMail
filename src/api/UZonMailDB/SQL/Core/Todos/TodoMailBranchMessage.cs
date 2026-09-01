using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.MailConversations;

namespace UzonMail.DB.SQL.Core.Todos;

/// <summary>
/// 待办新邮件线程产生的实际往来邮件。
/// </summary>
public sealed class TodoMailBranchMessage : SqlId, IEntityTypeConfiguration<TodoMailBranchMessage>
{
    public long TodoMailBranchId { get; set; }
    public TodoMailBranch TodoMailBranch { get; set; } = null!;
    public long MailConversationMessageId { get; set; }
    public MailConversationMessage MailConversationMessage { get; set; } = null!;

    public void Configure(EntityTypeBuilder<TodoMailBranchMessage> builder)
    {
        builder.ToTable("TodoMailBranchMessages");
        builder.HasIndex(x => x.MailConversationMessageId).IsUnique();
        builder.HasIndex(x => new { x.TodoMailBranchId, x.MailConversationMessageId }).IsUnique();
        builder
            .HasOne(x => x.TodoMailBranch)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.TodoMailBranchId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.MailConversationMessage)
            .WithMany()
            .HasForeignKey(x => x.MailConversationMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
