using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UZonMail.DB.Extensions
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 使用常见的分割符分割字符串
        /// 分割符有: 空格, 逗号, 分号, 冒号, 竖线
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] SplitBySeparators(this string? str)
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<string>();

            // 将常见的分割符替换成逗号
            var regex = new Regex(@"[\s,;:|，；：/]+");
            return regex.Replace(str, ",").Split(",");
        }
    }
}
