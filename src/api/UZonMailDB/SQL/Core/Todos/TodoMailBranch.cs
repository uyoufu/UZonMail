using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.MailConversations;

namespace UzonMail.DB.SQL.Core.Todos;

/// <summary>
/// 从主会话派生、但使用独立 RFC 根线程继续沟通的待办分支。
/// </summary>
public sealed class TodoMailBranch : SqlId, IEntityTypeConfiguration<TodoMailBranch>
{
    public long TodoTaskId { get; set; }
    public TodoTask TodoTask { get; set; } = null!;
    public long SourceConversationId { get; set; }
    public MailConversation SourceConversation { get; set; } = null!;
    public string BranchSubject { get; set; } = string.Empty;
    public long? RootSendingItemId { get; set; }
    public List<TodoMailBranchSourceMessage> SourceMessages { get; set; } = [];
    public List<TodoMailBranchMessage> Messages { get; set; } = [];

    public void Configure(EntityTypeBuilder<TodoMailBranch> builder)
    {
        builder.ToTable("TodoMailBranches");
        builder.Property(x => x.BranchSubject).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => x.TodoTaskId).IsUnique();
        builder.HasIndex(x => new { x.SourceConversationId, x.Id });
        builder
            .HasOne(x => x.TodoTask)
            .WithOne(x => x.MailBranch)
            .HasForeignKey<TodoMailBranch>(x => x.TodoTaskId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.SourceConversation)
            .WithMany()
            .HasForeignKey(x => x.SourceConversationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
