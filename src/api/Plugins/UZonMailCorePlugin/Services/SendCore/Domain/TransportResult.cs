namespace UzonMail.CorePlugin.Services.SendCore.Domain;

public enum SendFailureKind
{
    None,
    HardBounce,
    RecipientPermanent,
    MessagePermanent,
    OutboxPermanent,
    Transient,
    Network,
    Proxy,
    LocalData,
    Cancelled,
    Unknown,
}

public sealed record TransportResult(
    bool IsSuccess,
    SendFailureKind FailureKind,
    string Message,
    int? ProtocolStatusCode = null,
    string? ErrorCode = null,
    string? RejectedRecipientEmail = null,
    string? ReceiptId = null
)
{
    public static TransportResult Success(string? receiptId = null, string message = "success") =>
        new(true, SendFailureKind.None, message, ReceiptId: receiptId);

    public static TransportResult Failure(
        SendFailureKind kind,
        string message,
        int? protocolStatusCode = null,
        string? errorCode = null,
        string? rejectedRecipientEmail = null
    ) => new(false, kind, message, protocolStatusCode, errorCode, rejectedRecipientEmail);
}
