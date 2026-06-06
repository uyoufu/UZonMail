namespace UZonMail.CorePlugin.Controllers.Statistics.Model
{
    /// <summary>
    /// 邮箱数量
    /// </summary>
    public class EmailCount
    {
        /// <summary>
        /// 邮箱类型
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
    }
}
