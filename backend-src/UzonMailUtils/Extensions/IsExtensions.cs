using System;
using System.Collections.Generic;
using System.Text;

namespace UZonMail.Utils.Extensions
{
    public static class IsExtensions
    {
        /// <summary>
        /// 是否是邮箱
        /// </summary>
        /// <param name="emailStr"></param>
        /// <returns></returns>
        public static bool IsEmail(this string emailStr)
        {
            // 使用正则验证是否是邮箱
            var regex = new System.Text.RegularExpressions.Regex(@"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$");
            return regex.IsMatch(emailStr);
        }
    }
}
