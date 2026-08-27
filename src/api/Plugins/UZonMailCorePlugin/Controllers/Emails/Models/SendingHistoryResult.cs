using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Controllers.Emails.Models
{
    /// <summary>
    /// 发件历史结果
    /// </summary>
    public sealed class SendingHistoryResult
    {
        public long Id { get; }
        public string ObjectId { get; }
        public string Subjects { get; }
        public int TemplatesCount { get; }
        public int SenderAccountCount { get; }
        public int RecipientCount { get; }
        public int CcBoxesCount { get; }
        public int BccBoxesCount { get; }
        public SendingGroupStatus Status { get; }
        public string? StatusReason { get; }
        public DateTime? ResumeAtUtc { get; }
        public SendingGroupType SendingType { get; }
        public DateTime CreateDate { get; }
        public DateTime SendStartDate { get; }
        public DateTime SendEndDate { get; }
        public DateTime ScheduleDate { get; }
        public int TotalCount { get; }
        public int SuccessCount { get; }
        public int SentCount { get; }

        public SendingHistoryResult(SendingGroup sendingGroup)
        {
            Id = sendingGroup.Id;
            ObjectId = sendingGroup.ObjectId;
            Subjects = sendingGroup.Subjects;
            TemplatesCount = sendingGroup.Templates != null ? sendingGroup.Templates.Count : 0;
            SenderAccountCount = sendingGroup.SenderAccountCount;
            if (SenderAccountCount == 0)
                SenderAccountCount =
                    sendingGroup.SenderAccounts != null ? sendingGroup.SenderAccounts.Count : 0;
            RecipientCount = sendingGroup.RecipientCount;
            if (RecipientCount == 0)
                RecipientCount =
                    sendingGroup.Recipients != null ? sendingGroup.Recipients.Count : 0;
            CcBoxesCount = sendingGroup.CcBoxes != null ? sendingGroup.CcBoxes.Count : 0;
            BccBoxesCount = sendingGroup.BccBoxes != null ? sendingGroup.BccBoxes.Count : 0;
            Status = sendingGroup.Status;
            StatusReason = sendingGroup.StatusReason;
            ResumeAtUtc = sendingGroup.ResumeAtUtc;
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
