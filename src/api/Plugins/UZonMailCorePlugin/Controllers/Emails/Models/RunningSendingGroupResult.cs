using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Controllers.Emails.Models
{
    public class RunningSendingGroupResult
    {
        public long Id { get; set; }
        public string Subjects { get; set; }

        public double Progress { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public double TotalCount { get; set; }
        public int SentCount { get; set; }
        public int SuccessCount { get; set; }
        public SendingGroupStatus Status { get; set; }
        public string? StatusReason { get; set; }
        public DateTime? ResumeAtUtc { get; set; }

        public RunningSendingGroupResult(SendingGroup group)
        {
            Id = group.Id;
            Subjects = group.Subjects;
            TotalCount = group.TotalCount;
            SentCount = group.SentCount;
            SuccessCount = group.SuccessCount;
            Status = group.Status;
            StatusReason = group.StatusReason;
            ResumeAtUtc = group.ResumeAtUtc;
            Progress = SentCount * 1.0 / TotalCount;
        }
    }
}
