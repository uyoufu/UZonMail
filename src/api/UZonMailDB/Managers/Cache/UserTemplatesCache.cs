using System.Collections;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 用户自有模板集合的变更范围。
/// </summary>
public readonly record struct OwnedTemplateScope(long UserId);

/// <summary>
/// 直接共享给用户的模板集合变更范围。
/// </summary>
public readonly record struct SharedUserTemplateScope(long UserId);

/// <summary>
/// 共享给组织的模板集合变更范围。
/// </summary>
public readonly record struct SharedOrganizationTemplateScope(long OrganizationId);

/// <summary>
/// 用户当前可使用的邮件模板派生缓存。
/// </summary>
public sealed class UserTemplatesCache : BaseDBCache<SqlContext, long>, IEnumerable<EmailTemplate>
{
    private IReadOnlyList<EmailTemplate> _templates = [];

    public long UserId => Args;

    /// <summary>
    /// 获取用户自有模板集合的源键。
    /// </summary>
    public static CacheSourceKey<CacheRevision, OwnedTemplateScope> GetOwnedSourceKey(
        long userId
    ) => new(new OwnedTemplateScope(userId));

    /// <summary>
    /// 获取直接共享给用户的模板集合源键。
    /// </summary>
    public static CacheSourceKey<CacheRevision, SharedUserTemplateScope> GetSharedUserSourceKey(
        long userId
    ) => new(new SharedUserTemplateScope(userId));

    /// <summary>
    /// 获取组织共享模板集合的源键。
    /// </summary>
    public static CacheSourceKey<
        CacheRevision,
        SharedOrganizationTemplateScope
    > GetSharedOrganizationSourceKey(long organizationId) =>
        new(new SharedOrganizationTemplateScope(organizationId));

    /// <summary>
    /// 标记模板变更影响到的所有精确共享范围。
    /// </summary>
    public static async Task InvalidateTemplateScopesAsync(
        IDBCacheManager cacheManager,
        long ownerUserId,
        IEnumerable<long> sharedUserIds,
        IEnumerable<long> sharedOrganizationIds,
        CancellationToken cancellationToken = default
    )
    {
        await cacheManager.InvalidateSourceAsync(GetOwnedSourceKey(ownerUserId), cancellationToken);
        foreach (var sharedUserId in sharedUserIds.Distinct())
        {
            await cacheManager.InvalidateSourceAsync(
                GetSharedUserSourceKey(sharedUserId),
                cancellationToken
            );
        }

        foreach (var organizationId in sharedOrganizationIds.Distinct())
        {
            await cacheManager.InvalidateSourceAsync(
                GetSharedOrganizationSourceKey(organizationId),
                cancellationToken
            );
        }
    }

    /// <inheritdoc />
    protected override async Task UpdateCore(
        CacheBuildContext buildContext,
        SqlContext db,
        CancellationToken cancellationToken
    )
    {
        var userInfo = await UserInfoCache.GetSnapshotAsync(
            buildContext,
            db,
            UserId,
            cancellationToken
        );
        await ObserveRevisionAsync(buildContext, GetOwnedSourceKey(UserId), cancellationToken);
        await ObserveRevisionAsync(buildContext, GetSharedUserSourceKey(UserId), cancellationToken);
        await ObserveRevisionAsync(
            buildContext,
            GetSharedOrganizationSourceKey(userInfo.OrganizationId),
            cancellationToken
        );

        _templates = await db
            .EmailTemplates.AsNoTracking()
            .Where(x =>
                x.UserId == UserId
                || x.ShareToUsers.Select(sharedUser => sharedUser.Id).Contains(UserId)
                || x.ShareToOrganizations.Select(organization => organization.Id)
                    .Contains(userInfo.OrganizationId)
            )
            .ToListAsync(cancellationToken);
    }

    private static async Task ObserveRevisionAsync<TIdentity>(
        CacheBuildContext buildContext,
        CacheSourceKey<CacheRevision, TIdentity> sourceKey,
        CancellationToken cancellationToken
    )
        where TIdentity : notnull
    {
        await buildContext.GetSourceAsync(
            sourceKey,
            static _ => Task.FromResult(CacheRevision.Current),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public IEnumerator<EmailTemplate> GetEnumerator() => _templates.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
