namespace UZonMail.Core.SignalRHubs.SendEmail
{
    public class SendingProgressArg
    {
        /// <summary>
        /// 总数
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// 当前数量
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 开始时间
        /// 方便计算耗时
        /// </summary>
        public DateTime StartDate { get; set; }
    }
}
