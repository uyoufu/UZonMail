using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore;

/// <summary>
/// 校验准备后的邮件是否具备发送所需的最小数据。
/// </summary>
public sealed class PreparedSendItemValidator : ISingletonService
{
    /// <summary>
    /// 校验发件箱、收件人和正文等发送必需字段。
    /// </summary>
    public SendItemValidationResult Validate(PreparedSendItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Outbox.Email))
        {
            return SendItemValidationResult.Invalid(
                SendItemValidationFailure.MissingOutbox,
                "发件箱不存在"
            );
        }

        if (item.Inboxes.Count == 0)
        {
            return SendItemValidationResult.Invalid(
                SendItemValidationFailure.MissingRecipients,
                "收件箱为空"
            );
        }

        if (string.IsNullOrWhiteSpace(item.HtmlBody))
        {
            return SendItemValidationResult.Invalid(SendItemValidationFailure.MissingBody, "正文为空");
        }

        return SendItemValidationResult.Valid();
    }
}
