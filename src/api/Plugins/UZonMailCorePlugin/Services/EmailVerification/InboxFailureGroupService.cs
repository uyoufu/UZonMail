using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailVerification;

/// <summary>
/// 将已确认无效的收件箱集中迁移至失败分类
/// </summary>
public sealed class InboxFailureGroupService(SqlContext db) : IScopedService
{
    private const string ValidationFailedGroupName = "验证失败";
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> UserLocks = [];

    /// <summary>
    /// 标记收件箱无效并迁移到失败分类
    /// </summary>
    public async Task MarkInvalidAsync(
        long userId,
        IReadOnlyDictionary<long, string?> inboxFailureReasons,
        CancellationToken cancellationToken = default
    )
    {
        if (inboxFailureReasons.Count == 0)
            return;

        var userLock = UserLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await userLock.WaitAsync(cancellationToken);
        try
        {
            var failureGroup = await GetOrCreateFailureGroupAsync(userId, cancellationToken);
            var inboxIds = inboxFailureReasons.Keys.ToList();
            var inboxes = await db
                .Inboxes.Where(x => x.UserId == userId && inboxIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var inbox in inboxes)
            {
                inbox.EmailGroupId = failureGroup.Id;
                inbox.Status = InboxStatus.Invalid;
                inbox.ValidFailReason = inboxFailureReasons[inbox.Id];
            }
            await db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            userLock.Release();
        }
    }

    private async Task<EmailGroup> GetOrCreateFailureGroupAsync(
        long userId,
        CancellationToken cancellationToken
    )
    {
        var failureGroup = await db.EmailGroups.FirstOrDefaultAsync(
            x =>
                x.UserId == userId
                && x.Type == EmailGroupType.InBox
                && x.Name == ValidationFailedGroupName,
            cancellationToken
        );
        if (failureGroup != null)
            return failureGroup;

        failureGroup = new EmailGroup
        {
            UserId = userId,
            Type = EmailGroupType.InBox,
            Name = ValidationFailedGroupName,
            Description = "验证未通过的收件箱",
            Icon = "error",
            Order = long.MaxValue,
        };
        db.EmailGroups.Add(failureGroup);
        await db.SaveChangesAsync(cancellationToken);
        return failureGroup;
    }
}
