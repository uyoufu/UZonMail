using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailVerification;

/// <summary>
/// 将已确认无效的收件人集中迁移至失败分类。
/// </summary>
public sealed class RecipientContactFailureGroupService(SqlContext db) : IScopedService
{
    private const string ValidationFailedGroupName = "验证失败";
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> UserLocks = [];

    /// <summary>
    /// 标记收件箱无效并迁移到失败分类
    /// </summary>
    public async Task MarkInvalidAsync(
        long userId,
        IReadOnlyDictionary<long, string?> recipientContactFailureReasons,
        CancellationToken cancellationToken = default
    )
    {
        if (recipientContactFailureReasons.Count == 0)
            return;

        var userLock = UserLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await userLock.WaitAsync(cancellationToken);
        try
        {
            var failureGroup = await GetOrCreateFailureGroupAsync(userId, cancellationToken);
            var recipientContactIds = recipientContactFailureReasons.Keys.ToList();
            var recipientContacts = await db
                .RecipientContacts.Where(x =>
                    x.UserId == userId && recipientContactIds.Contains(x.Id)
                )
                .ToListAsync(cancellationToken);

            foreach (var recipientContact in recipientContacts)
            {
                recipientContact.EmailGroupId = failureGroup.Id;
                recipientContact.ValidationStatus = RecipientValidationStatus.Invalid;
                recipientContact.ValidationFailureReason = recipientContactFailureReasons[
                    recipientContact.Id
                ];
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
                && x.Category == EmailGroupCategory.Recipient
                && x.Name == ValidationFailedGroupName,
            cancellationToken
        );
        if (failureGroup != null)
            return failureGroup;

        failureGroup = new EmailGroup
        {
            UserId = userId,
            Category = EmailGroupCategory.Recipient,
            Name = ValidationFailedGroupName,
            Description = "验证未通过的收件人",
            Icon = "error",
            Order = 0,
        };
        db.EmailGroups.Add(failureGroup);
        await db.SaveChangesAsync(cancellationToken);
        return failureGroup;
    }
}
