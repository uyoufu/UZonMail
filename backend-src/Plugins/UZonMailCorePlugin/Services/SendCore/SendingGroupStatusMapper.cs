using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.Services.SendCore
{
    public static class SendingGroupStatusMapper
    {
        public static SendingItemStatus ToSendingItemStatus(SendingGroupStatus status)
        {
            return status switch
            {
                SendingGroupStatus.Cancel => SendingItemStatus.Cancel,
                SendingGroupStatus.Pause => SendingItemStatus.Failed,
                SendingGroupStatus.Finish => SendingItemStatus.Success,
                _ => SendingItemStatus.Failed,
            };
        }
    }
}
