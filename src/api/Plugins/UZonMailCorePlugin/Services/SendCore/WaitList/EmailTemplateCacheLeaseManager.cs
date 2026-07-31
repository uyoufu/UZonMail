using System.Collections.Concurrent;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Templates;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList;

/// <summary>
/// 协调发件组对模板结果缓存的租用。
/// 模板内容始终由 IDBCacheManager 管理；本服务只维护业务级引用、空闲回收和失效入口。
/// </summary>
public sealed class EmailTemplateCacheLeaseManager : ISingletonService, IAsyncDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(
        typeof(EmailTemplateCacheLeaseManager)
    );

    private readonly IDBCacheManager _cacheManager;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _idleTimeout;
    private readonly ConcurrentDictionary<long, EmailTemplateCacheReference> _references = [];
    private readonly Timer _cleanupTimer;
    private int _isCleanupRunning;
    private int _isDisposed;

    public EmailTemplateCacheLeaseManager(
        IDBCacheManager cacheManager,
        IOptions<DBCacheOptions> cacheOptions,
        TimeProvider timeProvider
    )
    {
        _cacheManager = cacheManager;
        _timeProvider = timeProvider;
        _idleTimeout = TimeSpan.FromMinutes(cacheOptions.Value.SlidingExpirationMinutes);
        var cleanupInterval = TimeSpan.FromMinutes(
            cacheOptions.Value.ExpirationScanFrequencyMinutes
        );
        _cleanupTimer = new Timer(
            static manager => ((EmailTemplateCacheLeaseManager)manager!).StartScheduledCleanup(),
            this,
            cleanupInterval,
            cleanupInterval
        );
    }

    /// <summary>
    /// 取得一个经当前用户权限验证的模板租约。
    /// 模板不存在或无访问权限时不会保留引用。
    /// </summary>
    internal async Task<EmailTemplateCacheLease?> AcquireByIdAsync(
        SqlContext sqlContext,
        long userId,
        long templateId,
        CancellationToken cancellationToken = default
    )
    {
        if (templateId <= 0)
            return null;

        ThrowIfDisposed();
        var cacheReference = AddReference(templateId);
        var lease = new EmailTemplateCacheLease(this, cacheReference, templateId, userId);
        try
        {
            if (await lease.GetTemplateAsync(sqlContext, cancellationToken) != null)
                return lease;
        }
        catch
        {
            await lease.DisposeAsync();
            throw;
        }

        await lease.DisposeAsync();
        return null;
    }

    /// <summary>
    /// 按名称解析当前用户可访问的模板 ID。
    /// 未命中不缓存，确保后续新增模板或权限调整可立即生效。
    /// </summary>
    internal async Task<long?> FindAccessibleTemplateIdByNameAsync(
        SqlContext sqlContext,
        long userId,
        string templateName,
        CancellationToken cancellationToken = default
    )
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(templateName))
            return null;

        var userInfo = await _cacheManager.GetCache<UserInfoCache>(
            sqlContext,
            userId,
            cancellationToken
        );
        return await sqlContext
            .EmailTemplates.AsNoTracking()
            .Where(template =>
                template.Name == templateName
                && (
                    template.UserId == userId
                    || template.ShareToUsers.Select(sharedUser => sharedUser.Id).Contains(userId)
                    || template
                        .ShareToOrganizations.Select(sharedOrganization => sharedOrganization.Id)
                        .Contains(userInfo.OrganizationId)
                )
            )
            .Select(template => (long?)template.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 标记模板正文或共享关系已变更，使下一次读取刷新全局模板快照。
    /// </summary>
    public Task MarkDirtyAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return templateId <= 0
            ? Task.CompletedTask
            : EmailTemplateCache.InvalidateAsync(_cacheManager, templateId, cancellationToken);
    }

    internal async Task<EmailTemplate?> GetTemplateAsync(
        SqlContext sqlContext,
        EmailTemplateCacheReference cacheReference,
        long templateId,
        long userId,
        CancellationToken cancellationToken
    )
    {
        ThrowIfDisposed();
        var userInfo = await _cacheManager.GetCache<UserInfoCache>(
            sqlContext,
            userId,
            cancellationToken
        );

        await cacheReference.CacheOperationLock.WaitAsync(cancellationToken);
        try
        {
            var cache = await _cacheManager.GetCache<EmailTemplateCache>(
                sqlContext,
                templateId,
                cancellationToken
            );
            if (!cache.CanAccess(userId, userInfo.OrganizationId))
                return null;

            cacheReference.MarkAccessed(_timeProvider.GetUtcNow());
            return cache.Template;
        }
        finally
        {
            cacheReference.CacheOperationLock.Release();
        }
    }

    internal async Task ReleaseAsync(long templateId, EmailTemplateCacheReference cacheReference)
    {
        if (!cacheReference.ReleaseReference())
            return;

        await cacheReference.CacheOperationLock.WaitAsync();
        try
        {
            if (!cacheReference.HasNoReferences())
                return;

            await _cacheManager.RemoveCacheAsync<EmailTemplateCache>(templateId);
            RemoveUnusedReference(templateId, cacheReference);
        }
        finally
        {
            cacheReference.CacheOperationLock.Release();
        }
    }

    internal Task CleanupIdleCachesAsync(DateTimeOffset utcNow) =>
        CleanupIdleCachesCoreAsync(utcNow);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            return;

        await _cleanupTimer.DisposeAsync();
        var references = _references.ToArray();
        _references.Clear();

        foreach (var (templateId, cacheReference) in references)
        {
            await cacheReference.CacheOperationLock.WaitAsync();
            try
            {
                await _cacheManager.RemoveCacheAsync<EmailTemplateCache>(templateId);
            }
            finally
            {
                cacheReference.CacheOperationLock.Release();
            }
        }
    }

    private EmailTemplateCacheReference AddReference(long templateId)
    {
        var cacheReference = _references.GetOrAdd(
            templateId,
            _ => new EmailTemplateCacheReference(_timeProvider.GetUtcNow())
        );
        cacheReference.AddReference(_timeProvider.GetUtcNow());
        return cacheReference;
    }

    private void StartScheduledCleanup()
    {
        if (Volatile.Read(ref _isDisposed) != 0)
            return;
        if (Interlocked.Exchange(ref _isCleanupRunning, 1) != 0)
            return;

        _ = CleanupScheduledCachesAsync();
    }

    private async Task CleanupScheduledCachesAsync()
    {
        try
        {
            await CleanupIdleCachesCoreAsync(_timeProvider.GetUtcNow());
        }
        catch (Exception exception)
        {
            Logger.Error("清理空闲模板缓存失败", exception);
        }
        finally
        {
            Interlocked.Exchange(ref _isCleanupRunning, 0);
        }
    }

    private async Task CleanupIdleCachesCoreAsync(DateTimeOffset utcNow)
    {
        foreach (var (templateId, cacheReference) in _references)
        {
            if (!cacheReference.IsIdle(utcNow, _idleTimeout))
                continue;

            await cacheReference.CacheOperationLock.WaitAsync();
            try
            {
                if (!cacheReference.IsIdle(utcNow, _idleTimeout))
                    continue;

                await _cacheManager.RemoveCacheAsync<EmailTemplateCache>(templateId);
                RemoveUnusedReference(templateId, cacheReference);
            }
            finally
            {
                cacheReference.CacheOperationLock.Release();
            }
        }
    }

    private void RemoveUnusedReference(long templateId, EmailTemplateCacheReference cacheReference)
    {
        if (cacheReference.HasNoReferences())
            _references.TryRemove(
                new KeyValuePair<long, EmailTemplateCacheReference>(templateId, cacheReference)
            );
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _isDisposed) != 0)
            throw new ObjectDisposedException(nameof(EmailTemplateCacheLeaseManager));
    }
}
