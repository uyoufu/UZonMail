using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs;

/// <summary>
/// 发件明细列表中的只读摘要。
/// </summary>
public sealed class SendingItemSummaryDto
{
    public long Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public List<EmailAddressDto> Recipients { get; set; } = [];
    public SendingItemStatus Status { get; set; }
    public DateTime SendDate { get; set; }
    public string? SendResult { get; set; }
}
