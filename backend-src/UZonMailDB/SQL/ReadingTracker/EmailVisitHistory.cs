using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.ReadingTracker
{
    /// <summary>
    /// 访问历史
    /// 创建时间即访问时间
    /// </summary>
    public class EmailVisitHistory : SqlId
    {
        /// <summary>
        /// IP 地址
        /// </summary>
        public string IP { get; set; }
    }
}
