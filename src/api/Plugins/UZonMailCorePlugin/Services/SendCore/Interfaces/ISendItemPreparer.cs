using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendItemPreparer
    {
        /// <summary>
        /// 将数据库载荷和发送配置组装为一次发送使用的只读快照。
        /// </summary>
        Task<PreparedSendItem> PrepareAsync(
            SendingContext sendingContext,
            SendingItem sendingItem,
            OutboxEmailAddress outbox,
            SendingGroup sendingGroup,
            SendingGroupTemplateResolver templateResolver,
            IReadOnlyList<long> proxyIds
        );
    }
}
