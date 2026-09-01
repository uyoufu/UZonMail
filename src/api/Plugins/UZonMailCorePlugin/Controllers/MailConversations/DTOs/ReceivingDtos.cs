using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.MailConversations.DTOs;

public sealed record ReceivingAccountSummaryDto(
    long EmailAccountId,
    long ReceivingAccountId,
    string Email,
    string? Name,
    ReceivingProtocol Protocol,
    ReceivingAccountStatus Status,
    DateTime? LastSuccessfulSyncAtUtc,
    string? LastError,
    bool IsSupported
);
