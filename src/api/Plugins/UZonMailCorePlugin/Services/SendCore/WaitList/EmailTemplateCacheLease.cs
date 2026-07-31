using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList;

/// <summary>
/// 发件组对全局模板缓存的使用凭证。
/// 租约不保存模板实体，因此模板失效后下一次读取总能获取最新缓存快照。
/// </summary>
internal sealed class EmailTemplateCacheLease(
    EmailTemplateCacheLeaseManager cacheLeaseManager,
    EmailTemplateCacheReference cacheReference,
    long templateId,
    long userId
) : IAsyncDisposable
{
    private int _isReleased;

    public long TemplateId { get; } = templateId;

    public Task<EmailTemplate?> GetTemplateAsync(
        SqlContext sqlContext,
        CancellationToken cancellationToken = default
    )
    {
        ThrowIfReleased();
        return cacheLeaseManager.GetTemplateAsync(
            sqlContext,
            cacheReference,
            TemplateId,
            userId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return Interlocked.Exchange(ref _isReleased, 1) == 0
            ? new ValueTask(cacheLeaseManager.ReleaseAsync(TemplateId, cacheReference))
            : ValueTask.CompletedTask;
    }

    private void ThrowIfReleased()
    {
        if (Volatile.Read(ref _isReleased) != 0)
            throw new ObjectDisposedException(nameof(EmailTemplateCacheLease));
    }
}
