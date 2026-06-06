using System;

namespace UZonMail.Utils.Extensions
{
    /// <summary>
    /// 时间相关的扩展
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime dateTime)
        {
           return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(dateTime)).ToUnixTimeMilliseconds();
        }
    }
}
