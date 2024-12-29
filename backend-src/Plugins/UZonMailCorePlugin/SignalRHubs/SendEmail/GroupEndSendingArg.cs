using UZonMail.DB.SQL.EmailSending;

namespace UZonMail.Core.SignalRHubs.SendEmail
{
    public class GroupEndSendingArg
    {
        public GroupEndSendingArg(SendingGroup sendingGroup, DateTime startDate)
        {
            StartDate = startDate;
            SendingGroupId = sendingGroup.Id;
            Total = sendingGroup.TotalCount;
            Success = sendingGroup.SuccessCount;
        }

        public DateTime StartDate { get; set; }
        public long SendingGroupId { get; set; }
        public int Total { get; set; }
        public int Success { get; set; }
        public string Message { get; set;}
    }
}
