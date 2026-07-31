using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList;

/// <summary>
/// 解析单个发件组可使用的模板。
/// 该对象仅保存组内模板选择与租约；模板内容、权限快照和缓存回收由全局租约管理器协调。
/// </summary>
public sealed class SendingGroupTemplateResolver(
    long userId,
    EmailTemplateCacheLeaseManager cacheLeaseManager
) : IAsyncDisposable
{
    private readonly ConcurrentDictionary<long, long> _sendingItemTemplateIds = [];
    private readonly List<long> _sendingGroupTemplateIds = [];
    private readonly ConcurrentDictionary<
        long,
        Lazy<Task<EmailTemplateCacheLease?>>
    > _templateLeases = [];
    private readonly ConcurrentDictionary<string, long> _templateIdsByName =
        new(StringComparer.Ordinal);
    private int _isDisposed;

    /// <summary>
    /// 为发件项绑定指定模板。
    /// </summary>
    public void AddSendingItemTemplate(long sendingItemId, long templateId)
    {
        ThrowIfDisposed();
        if (templateId > 0)
            _sendingItemTemplateIds.TryAdd(sendingItemId, templateId);
    }

    /// <summary>
    /// 添加发件组的随机模板候选项。
    /// 此方法仅在组初始化阶段调用，之后候选列表保持只读以支持并发解析。
    /// </summary>
    public void AddSendingGroupTemplates(IEnumerable<long> templateIds)
    {
        ThrowIfDisposed();
        _sendingGroupTemplateIds.AddRange(templateIds.Where(templateId => templateId > 0));
    }

    /// <summary>
    /// 获取发件项绑定的模板；未绑定时从发件组候选模板中随机选择。
    /// </summary>
    public async Task<EmailTemplate?> GetTemplate(SqlContext sqlContext, long sendingItemId)
    {
        ThrowIfDisposed();
        if (_sendingItemTemplateIds.TryGetValue(sendingItemId, out var templateId))
            return await GetTemplateById(sqlContext, templateId);

        if (_sendingGroupTemplateIds.Count == 0)
            return null;

        var randomIndex = RandomNumberGenerator.GetInt32(0, _sendingGroupTemplateIds.Count);
        return await GetTemplateById(sqlContext, _sendingGroupTemplateIds[randomIndex]);
    }

    /// <summary>
    /// 通过模板 ID 获取当前用户有权使用的模板。
    /// </summary>
    public async Task<EmailTemplate?> GetTemplateById(SqlContext sqlContext, long templateId)
    {
        var lease = await GetLeaseAsync(sqlContext, templateId);
        return lease == null ? null : await lease.GetTemplateAsync(sqlContext);
    }

    /// <summary>
    /// 通过模板名称获取当前用户有权使用的模板。
    /// 只有成功解析的名称会在发件组内缓存，避免新增模板被旧的空结果阻塞。
    /// </summary>
    public async Task<EmailTemplate?> GetTemplateByName(SqlContext sqlContext, string templateName)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(templateName))
            return null;

        if (_templateIdsByName.TryGetValue(templateName, out var cachedTemplateId))
        {
            var cachedTemplate = await GetTemplateById(sqlContext, cachedTemplateId);
            if (
                cachedTemplate != null
                && StringComparer.Ordinal.Equals(cachedTemplate.Name, templateName)
            )
                return cachedTemplate;

            _templateIdsByName.TryRemove(templateName, out _);
        }

        var templateId = await cacheLeaseManager.FindAccessibleTemplateIdByNameAsync(
            sqlContext,
            userId,
            templateName
        );
        if (templateId is not > 0)
            return null;

        _templateIdsByName.TryAdd(templateName, templateId.Value);
        return await GetTemplateById(sqlContext, templateId.Value);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            return;

        var leaseTasks = _templateLeases
            .Values.Where(leaseFactory => leaseFactory.IsValueCreated)
            .Select(leaseFactory => leaseFactory.Value)
            .ToArray();
        _templateLeases.Clear();
        _templateIdsByName.Clear();
        _sendingItemTemplateIds.Clear();
        _sendingGroupTemplateIds.Clear();

        ExceptionDispatchInfo? firstException = null;
        foreach (var leaseTask in leaseTasks)
        {
            try
            {
                var lease = await leaseTask;
                if (lease != null)
                    await lease.DisposeAsync();
            }
            catch (Exception exception)
            {
                firstException ??= ExceptionDispatchInfo.Capture(exception);
            }
        }

        firstException?.Throw();
    }

    private async Task<EmailTemplateCacheLease?> GetLeaseAsync(
        SqlContext sqlContext,
        long templateId
    )
    {
        ThrowIfDisposed();
        if (templateId <= 0)
            return null;

        var newLeaseFactory = new Lazy<Task<EmailTemplateCacheLease?>>(
            () => cacheLeaseManager.AcquireByIdAsync(sqlContext, userId, templateId),
            LazyThreadSafetyMode.ExecutionAndPublication
        );
        var leaseFactory = _templateLeases.GetOrAdd(templateId, newLeaseFactory);
        try
        {
            var lease = await leaseFactory.Value;
            if (lease == null)
                RemoveLeaseFactory(templateId, leaseFactory);
            return lease;
        }
        catch
        {
            RemoveLeaseFactory(templateId, leaseFactory);
            throw;
        }
    }

    private void RemoveLeaseFactory(
        long templateId,
        Lazy<Task<EmailTemplateCacheLease?>> leaseFactory
    )
    {
        _templateLeases.TryRemove(
            new KeyValuePair<long, Lazy<Task<EmailTemplateCacheLease?>>>(templateId, leaseFactory)
        );
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _isDisposed) != 0)
            throw new ObjectDisposedException(nameof(SendingGroupTemplateResolver));
    }
}
