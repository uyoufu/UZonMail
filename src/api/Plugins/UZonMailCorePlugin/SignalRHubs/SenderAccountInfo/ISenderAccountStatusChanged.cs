using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.SignalRHubs.SenderAccountInfo
{
    /// <summary>
    /// 发件箱状态变更
    /// </summary>
    public interface ISenderAccountStatusChanged
    {
        Task SenderAccountStatusChanged(SenderAccount senderAccount);

        Task RecipientContactStatusChanged(RecipientContact recipientContact);
    }
}
