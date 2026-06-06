using UZonMail.CorePlugin.Database.SQL.EmailSending;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.EmailWaitList;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
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
