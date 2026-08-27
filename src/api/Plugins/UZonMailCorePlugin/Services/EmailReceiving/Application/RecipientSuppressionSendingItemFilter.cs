using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailReceiving.Application;

/// <summary>
/// 在发送项进入执行队列前阻止命中组织停发名单的收件人。
/// </summary>
public sealed class RecipientSuppressionSendingItemFilter(
    IRecipientSuppressionService suppressionService
) : ISendingItemFilter, IScopedService<ISendingItemFilter>
{
    public async Task<List<long>> GetInvalidSendingItemIds(List<SendingItem> sendingItems)
    {
        List<long> invalidIds = [];
        foreach (var organizationItems in sendingItems.GroupBy(x => x.OrganizationId))
        {
            var suppressedEmails = await suppressionService.GetSuppressedEmailsAsync(
                organizationItems.Key,
                organizationItems.SelectMany(x => x.Recipients).Select(x => x.Email)
            );
            invalidIds.AddRange(
                organizationItems
                    .Where(x => x.Recipients.Any(r => suppressedEmails.Contains(r.Email)))
                    .Select(x => x.Id)
            );
        }
        return invalidIds;
    }
}
