using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Controllers.Emails.Models
{
    /// <summary>
    /// 发件历史结果
    /// </summary>
    public class SendingHistoryResult : SendingGroup
    {
        public int TemplatesCount { get; }
        public int CcBoxesCount { get; }
        public int BccBoxesCount { get; }

        public SendingHistoryResult(SendingGroup sendingGroup)
        {
            Id = sendingGroup.Id;
            ObjectId = sendingGroup.ObjectId;
            UserId = sendingGroup.UserId;
            Subjects = sendingGroup.Subjects;
            TemplatesCount = sendingGroup.Templates != null ? sendingGroup.Templates.Count : 0;
            OutboxesCount = sendingGroup.OutboxesCount;
            if (OutboxesCount == 0)
                OutboxesCount = sendingGroup.Outboxes != null ? sendingGroup.Outboxes.Count : 0;
            InboxesCount = sendingGroup.InboxesCount;
            if (InboxesCount == 0)
                InboxesCount = sendingGroup.Inboxes != null ? sendingGroup.Inboxes.Count : 0;
            CcBoxesCount = sendingGroup.CcBoxes != null ? sendingGroup.CcBoxes.Count : 0;
            BccBoxesCount = sendingGroup.BccBoxes != null ? sendingGroup.BccBoxes.Count : 0;
            Attachments = sendingGroup.Attachments;
            Status = sendingGroup.Status;
            SendingType = sendingGroup.SendingType;
            CreateDate = sendingGroup.CreateDate;
            SendStartDate = sendingGroup.SendStartDate;
            SendEndDate = sendingGroup.SendEndDate;
            ScheduleDate = sendingGroup.ScheduleDate;
            TotalCount = sendingGroup.TotalCount;
            SuccessCount = sendingGroup.SuccessCount;
            SentCount = sendingGroup.SentCount;            
        }
    }
}
