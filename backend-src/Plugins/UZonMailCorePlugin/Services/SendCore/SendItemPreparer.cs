using UZonMail.CorePlugin.Database.SQL.EmailSending;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.EmailWaitList;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore
{
    public class SendItemPreparer(AppSettingsManager settingsService)
        : ISendItemPreparer,
            IScopedService<ISendItemPreparer>
    {
        public async Task Prepare(
            SendingContext sendingContext,
            SendItemMeta sendItemMeta,
            SendingGroup sendingGroup,
            UsableTemplateList usableTemplates,
            IReadOnlyList<long> proxyIds
        )
        {
            var sqlContext = sendingContext.SqlContext;
            sendItemMeta.AvailableProxyIds = [.. proxyIds];

            await sendItemMeta.ResolveAttachments(sendingContext);

            var originBody = await GetSendingItemOriginBody(
                sqlContext,
                sendingGroup,
                usableTemplates,
                sendItemMeta.SendingItem,
                sendItemMeta.BodyData
            );
            await sendItemMeta.SetHtmlBody(sendingContext, originBody);

            var subject = GetSubject(sendingGroup, sendItemMeta.BodyData);
            await sendItemMeta.SetSubject(sendingContext, subject);

            var sendingSetting = await settingsService.GetSetting<SendingSetting>(
                sqlContext,
                sendItemMeta.SendingItem.UserId
            );
            await sendItemMeta.SetMaxRetryCount(sendingSetting.MaxRetryCount);
        }

        public async Task<string> GetSendingItemOriginBody(
            SqlContext sqlContext,
            SendingGroup sendingGroup,
            UsableTemplateList usableTemplates,
            SendingItem sendingItem,
            SendingItemExcelData? bodyData
        )
        {
            if (sendingItem.IsSendingBatch)
            {
                if (!string.IsNullOrEmpty(sendingGroup.Body))
                    return sendingGroup.Body;

                var template = await usableTemplates.GetTemplate(sqlContext, sendingItem.Id);
                return template?.Content ?? string.Empty;
            }

            if (bodyData != null)
            {
                if (!string.IsNullOrEmpty(bodyData.Body))
                    return bodyData.Body;

                if (bodyData.TemplateId > 0)
                {
                    var template = await usableTemplates.GetTemplateById(
                        sqlContext,
                        bodyData.TemplateId
                    );
                    if (template != null)
                        return template.Content;
                }

                if (!string.IsNullOrEmpty(bodyData.TemplateName))
                {
                    var template = await usableTemplates.GetTemplateByName(
                        sqlContext,
                        bodyData.TemplateName
                    );
                    if (template != null)
                        return template.Content;
                }
            }

            if (!string.IsNullOrEmpty(sendingGroup.Body))
                return sendingGroup.Body;

            var randTemplate = await usableTemplates.GetTemplate(sqlContext, sendingItem.Id);
            return randTemplate?.Content ?? string.Empty;
        }

        public string GetSubject(SendingGroup sendingGroup, SendingItemExcelData? bodyData)
        {
            if (bodyData != null && !string.IsNullOrEmpty(bodyData.Subject))
            {
                return bodyData.Subject;
            }

            return sendingGroup.GetRandSubject();
        }
    }
}
