using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.SignalRHubs.SendEmail
{
    /// <summary>
    /// 发送组进度参数
    /// </summary>
    public class SendingGroupProgressArg : SendingProgressArg
    {
        public SendingGroupProgressArg(SendingGroup sendingGroup, DateTime startDate)
        {
            Status = sendingGroup.Status;
            Total = sendingGroup.TotalCount;
            Current = sendingGroup.SentCount;
            StartDate = startDate;
            SendingGroupId = sendingGroup.Id;
            SuccessCount = sendingGroup.SuccessCount;
            SentCount = sendingGroup.SentCount;
            Subject = sendingGroup.GetFirstSubject();
        }

        /// <summary>
        /// 发件组 id
        /// </summary>
        public long SendingGroupId { get; set; }

        public SendingGroupStatus Status { get; set; }

        /// <summary>
        /// 成功的数量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 已发送的数量
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// 单个邮件主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 进度类型
        /// </summary>
        public ProgressType ProgressType { get; set; } = ProgressType.Sending;
    }
}
