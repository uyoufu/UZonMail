namespace UZonMail.CorePlugin.Controllers.Statistics.Model
{
    /// <summary>
    /// 每月发送详细
    /// </summary>
    public class MonthlySendingInfo
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
    }
}
