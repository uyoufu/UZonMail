using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.ReadingTracker
{
    [Index(nameof(IP))]
    public class IPInfo : SqlId
    {
        /// <summary>
        /// 主机
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string? Country { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? ISP { get; set; }
        public string? PostalCode { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? TimeZone { get; set; }
        public string? UsageType { get; set; }
    }
}
