using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.EmailWaitList;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendItemPreparer
    {
        Task Prepare(
            SendingContext sendingContext,
            SendItemMeta sendItemMeta,
            SendingGroup sendingGroup,
            UsableTemplateList usableTemplates,
            IReadOnlyList<long> proxyIds
        );

        Task<string> GetSendingItemOriginBody(
            SqlContext sqlContext,
            SendingGroup sendingGroup,
            UsableTemplateList usableTemplates,
            SendingItem sendingItem,
            SendingItemExcelData? bodyData
        );

        string GetSubject(SendingGroup sendingGroup, SendingItemExcelData? bodyData);
    }
}
