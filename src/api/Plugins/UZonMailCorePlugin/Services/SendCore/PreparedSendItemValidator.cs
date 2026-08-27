using MimeKit;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore;

/// <summary>
/// 校验准备后的邮件是否具备发送所需的最小数据。
/// </summary>
public sealed class PreparedSendItemValidator : ISingletonService
{
    private static readonly ParserOptions StrictAddressParserOptions =
        new()
        {
            AddressParserComplianceMode = RfcComplianceMode.Strict,
            AllowAddressesWithoutDomain = false,
        };

    /// <summary>
    /// 校验发件箱、收件人和正文等发送必需字段。
    /// </summary>
    public SendItemValidationResult Validate(PreparedSendItem item)
    {
        if (string.IsNullOrWhiteSpace(item.SenderAccount.Email))
        {
            return SendItemValidationResult.Invalid(
                SendItemValidationFailure.MissingSenderAccount,
                "发件箱不存在"
            );
        }

        var addressValidation = ValidateAddress("发件箱", item.SenderAccount.Email);
        if (addressValidation != null)
            return addressValidation;

        if (item.Recipients.Count == 0)
        {
            return SendItemValidationResult.Invalid(
                SendItemValidationFailure.MissingRecipients,
                "收件箱为空"
            );
        }

        addressValidation = ValidateAddresses("收件人", item.Recipients.Select(x => x.Email));
        if (addressValidation != null)
            return addressValidation;

        addressValidation = ValidateAddresses(
            "抄送",
            item.CC.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email)
        );
        if (addressValidation != null)
            return addressValidation;

        addressValidation = ValidateAddresses(
            "密送",
            item.BCC.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email)
        );
        if (addressValidation != null)
            return addressValidation;

        addressValidation = ValidateAddresses("回复地址", item.ReplyToEmails);
        if (addressValidation != null)
            return addressValidation;

        if (string.IsNullOrWhiteSpace(item.HtmlBody))
        {
            return SendItemValidationResult.Invalid(SendItemValidationFailure.MissingBody, "正文为空");
        }

        return SendItemValidationResult.Valid();
    }

    private static SendItemValidationResult? ValidateAddresses(
        string addressRole,
        IEnumerable<string> emailAddresses
    )
    {
        foreach (var emailAddress in emailAddresses)
        {
            var validation = ValidateAddress(addressRole, emailAddress);
            if (validation != null)
                return validation;
        }

        return null;
    }

    private static SendItemValidationResult? ValidateAddress(
        string addressRole,
        string emailAddress
    )
    {
        if (
            !string.IsNullOrWhiteSpace(emailAddress)
            && MailboxAddress.TryParse(
                StrictAddressParserOptions,
                emailAddress,
                out var mailboxAddress
            )
            && string.Equals(mailboxAddress.Address, emailAddress, StringComparison.Ordinal)
        )
            return null;

        return SendItemValidationResult.Invalid(
            SendItemValidationFailure.InvalidEmailAddress,
            $"{addressRole}邮箱格式不正确：{emailAddress}"
        );
    }
}
