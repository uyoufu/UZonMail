using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.EmailDecorator;
using UzonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore;

/// <summary>
/// 将数据库载荷、模板、设置和附件组装为不可变的发送快照。
/// </summary>
public sealed class SendItemPreparer(
    AppSettingsManager settingsService,
    SendAttachmentResolver attachmentResolver,
    EmailContentDecorateService contentDecorateService
) : ISendItemPreparer, IScopedService<ISendItemPreparer>
{
    /// <inheritdoc />
    public async Task<PreparedSendItem> PrepareAsync(
        SendingContext sendingContext,
        SendingItem sendingItem,
        SenderEmailAddress senderAccount,
        SendingGroup sendingGroup,
        SendingGroupTemplateResolver templateResolver,
        IReadOnlyList<long> proxyIds
    )
    {
        var db = sendingContext.SqlContext;
        var variables = new SendingItemExcelData(sendingItem.Data);
        var setting = await settingsService.GetSetting<SendingSetting>(db, sendingItem.UserId);
        var attachments = await attachmentResolver.ResolveAsync(sendingItem);
        var originBody = await GetOriginBodyAsync(
            db,
            sendingGroup,
            templateResolver,
            sendingItem,
            variables
        );
        var originSubject = GetSubject(sendingGroup, variables);

        var bodyParams = new EmailDecoratorParams(
            setting,
            sendingItem,
            variables,
            senderAccount.SenderAccount,
            originSubject,
            originBody
        );
        var htmlBody = await contentDecorateService.Decorate(bodyParams, originBody);
        var subjectParams = new EmailDecoratorParams(
            setting,
            sendingItem,
            variables,
            senderAccount.SenderAccount,
            originSubject,
            htmlBody
        );
        var subject = await contentDecorateService.ResolveVariables(subjectParams, originSubject);
        var replyRecipientEmails =
            senderAccount.ReplyToEmails.Count > 0
                ? senderAccount.ReplyToEmails
                : setting.ReplyToEmailsList;

        return new PreparedSendItem(
            sendingItem,
            senderAccount,
            variables,
            subject,
            htmlBody,
            attachments,
            [.. replyRecipientEmails],
            [.. proxyIds],
            setting
        );
    }

    private static async Task<string> GetOriginBodyAsync(
        SqlContext db,
        SendingGroup sendingGroup,
        SendingGroupTemplateResolver templateResolver,
        SendingItem sendingItem,
        SendingItemExcelData? variables
    )
    {
        if (sendingItem.IsSendingBatch)
        {
            if (!string.IsNullOrEmpty(sendingGroup.Body))
                return sendingGroup.Body;
            return (await templateResolver.GetTemplate(db, sendingItem.Id))?.Content
                ?? string.Empty;
        }

        if (variables != null)
        {
            if (!string.IsNullOrEmpty(variables.Body))
                return variables.Body;
            if (variables.TemplateId > 0)
            {
                var template = await templateResolver.GetTemplateById(db, variables.TemplateId);
                if (template != null)
                    return template.Content;
            }
            if (!string.IsNullOrEmpty(variables.TemplateName))
            {
                var template = await templateResolver.GetTemplateByName(db, variables.TemplateName);
                if (template != null)
                    return template.Content;
            }
        }

        if (!string.IsNullOrEmpty(sendingGroup.Body))
            return sendingGroup.Body;
        return (await templateResolver.GetTemplate(db, sendingItem.Id))?.Content ?? string.Empty;
    }

    private static string GetSubject(SendingGroup sendingGroup, SendingItemExcelData? variables) =>
        variables != null && !string.IsNullOrEmpty(variables.Subject)
            ? variables.Subject
            : sendingGroup.GetRandSubject();
}
