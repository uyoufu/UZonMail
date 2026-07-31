using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 单个邮件模板变更的修订范围。
/// 模板内容和共享关系使用同一修订，确保授权判断不会滞后于数据库写入。
/// </summary>
public readonly record struct EmailTemplateRevisionScope(long TemplateId);

/// <summary>
/// 按模板 ID 缓存邮件内容和共享访问元数据。
/// 读取方必须通过 <see cref="CanAccess"/> 校验权限，避免全局缓存绕过访问控制。
/// </summary>
public sealed class EmailTemplateCache : BaseDBCache<SqlContext, long>
{
    private EmailTemplate? _template;

    /// <summary>
    /// 当前缓存对应的模板 ID。
    /// </summary>
    public long TemplateId => Args;

    /// <summary>
    /// 当前模板快照；模板已删除或不存在时为 null。
    /// </summary>
    public EmailTemplate? Template => _template;

    /// <summary>
    /// 获取模板变更的原始修订键。
    /// </summary>
    public static CacheSourceKey<CacheRevision, EmailTemplateRevisionScope> GetSourceKey(
        long templateId
    ) => new(new EmailTemplateRevisionScope(templateId));

    /// <summary>
    /// 标记模板内容或共享关系已变更，使下一次读取重新加载完整快照。
    /// </summary>
    public static Task InvalidateAsync(
        IDBCacheManager cacheManager,
        long templateId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(cacheManager);
        return cacheManager.InvalidateSourceAsync(GetSourceKey(templateId), cancellationToken);
    }

    /// <summary>
    /// 判断用户是否可以使用当前模板。
    /// 共享关系与内容由同一缓存快照加载，避免授权与正文版本不一致。
    /// </summary>
    public bool CanAccess(long userId, long organizationId)
    {
        return _template != null
            && (
                _template.UserId == userId
                || _template.ShareToUsers.Any(sharedUser => sharedUser.Id == userId)
                || _template.ShareToOrganizations.Any(sharedOrganization =>
                    sharedOrganization.Id == organizationId
                )
            );
    }

    /// <inheritdoc />
    protected override async Task UpdateCore(
        CacheBuildContext buildContext,
        SqlContext db,
        CancellationToken cancellationToken
    )
    {
        await buildContext.GetSourceAsync(
            GetSourceKey(TemplateId),
            static _ => Task.FromResult(CacheRevision.Current),
            cancellationToken
        );

        _template = await db
            .EmailTemplates.AsNoTracking()
            .Include(template => template.ShareToUsers)
            .Include(template => template.ShareToOrganizations)
            .FirstOrDefaultAsync(template => template.Id == TemplateId, cancellationToken);
    }
}
