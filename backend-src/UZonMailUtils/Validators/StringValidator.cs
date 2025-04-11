using System.Text.RegularExpressions;

namespace UZonMail.Utils.Validators
{
    public static class StringValidator
    {
        /// <summary>
        /// 是否是有效的邮箱格式
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))return false;

            // 邮箱正则表达式
            var pattern = @"^[a-zA-Z0-9_%+-]+(\.[a-zA-Z0-9_%+-]+)*@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
    }
}
