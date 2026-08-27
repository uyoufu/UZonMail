using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

/// <summary>
/// 持久化永久失效发件箱的业务状态。
/// </summary>
public sealed class SenderAccountInvalidationPersistenceService(SqlContext db) : IScopedService
{
    /// <summary>
    /// 发件箱被判定为永久失效时更新数据库状态。
    /// </summary>
    public Task PersistAsync(SenderEmailAddress senderAccount)
    {
        if (!senderAccount.IsPermanentlyInvalid)
            return Task.CompletedTask;

        return db.SenderAccounts.UpdateAsync(
            x => x.Id == senderAccount.Id,
            x =>
                x.SetProperty(y => y.Status, SenderAccountStatus.Invalid)
                    .SetProperty(y => y.ValidationFailureReason, senderAccount.ErroredMessage)
        );
    }
}
