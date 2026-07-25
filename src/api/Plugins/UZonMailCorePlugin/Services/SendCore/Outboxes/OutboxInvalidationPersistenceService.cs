using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Outboxes;

/// <summary>
/// 持久化永久失效发件箱的业务状态。
/// </summary>
public sealed class OutboxInvalidationPersistenceService(SqlContext db) : IScopedService
{
    /// <summary>
    /// 发件箱被判定为永久失效时更新数据库状态。
    /// </summary>
    public Task PersistAsync(OutboxEmailAddress outbox)
    {
        if (!outbox.IsPermanentlyInvalid)
            return Task.CompletedTask;

        return db.Outboxes.UpdateAsync(
            x => x.Id == outbox.Id,
            x =>
                x.SetProperty(y => y.IsValid, false)
                    .SetProperty(y => y.Status, OutboxStatus.Invalid)
                    .SetProperty(y => y.ValidFailReason, outbox.ErroredMessage)
        );
    }
}
