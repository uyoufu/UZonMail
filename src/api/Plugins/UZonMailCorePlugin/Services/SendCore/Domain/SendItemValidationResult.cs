namespace UzonMail.CorePlugin.Services.SendCore.Domain;

/// <summary>
/// 准备后邮件的校验失败类型。
/// </summary>
public enum SendItemValidationFailure
{
    None,
    MissingOutbox,
    MissingRecipients,
    MissingBody,
}

/// <summary>
/// 准备后邮件的校验结果。
/// </summary>
public sealed record SendItemValidationResult(
    bool IsValid,
    SendItemValidationFailure Failure,
    string Message
)
{
    public static SendItemValidationResult Valid() =>
        new(true, SendItemValidationFailure.None, string.Empty);

    public static SendItemValidationResult Invalid(
        SendItemValidationFailure failure,
        string message
    ) => new(false, failure, message);
}
