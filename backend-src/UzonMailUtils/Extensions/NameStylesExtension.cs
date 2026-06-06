using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UZonMail.Utils.Extensions
{
    /// <summary>
    /// 命名转换
    /// </summary>
    public static class NameStylesExtension
    {
        /// <summary>
        /// 命名样式
        /// </summary>
        public enum NameStylesType
        {
            CamelCase,

            SnakeCase,

            PascalCase,

            HyphenCase,
        }

        

        // 大写字母开头
        // 不是小写字母 或者是 大写字母
        private static readonly Regex WORD_REGEX = new Regex(@"[A-Z]+(?![a-z])|[A-Z](?=[a-z])", RegexOptions.Multiline);

        /// <summary>
        /// 转换成指定样式
        /// </summary>
        /// <param name="str"></param>
        /// <param name="nameStylesType"></param>
        /// <returns></returns>
        public static string ToNameStyle(this string str, NameStylesType nameStylesType)
        {
            return nameStylesType switch
            {
                NameStylesType.CamelCase => str.ToCamelCase(),
                NameStylesType.SnakeCase => str.ToSnakeCase(),
                NameStylesType.PascalCase => str.ToPascalCase(),
                NameStylesType.HyphenCase => str.ToHyphenCase(),
                _ => str,
            };
        }

        /// <summary>
        /// 转换成 camelLike 样式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ToCamelLike(this string str)
        {
            var temp = WORD_REGEX.Replace(str, m => $"_{m.Value}").ToLower();
            var regex = new Regex(@"[-_\s]+([a-z])");
            temp = regex.Replace(temp, m =>
            {
               return char.ToUpper(m.Value[m.Value.Length - 1]).ToString();
            });
            return temp;
        }

        /// <summary>
        /// 转换成 snake_like 样式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ToSnakeLike(this string str)
        {
            var temp = WORD_REGEX.Replace(str, m => $"_{m.Value}");
            var regex1 = new Regex(@"[-_\s]+");
            var temp1 = regex1.Replace(temp, "_");
            var regex2 = new Regex(@"^_+");
            var temp2 = regex2.Replace(temp1, "");
            return temp2.ToLower();
        }

        /// <summary>
        /// 将字符串转换成 camelCase 命名样式<br/>
        /// 示例：nameStylesIsAGoodLibrary
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string str)
        {
            Regex regex = new Regex("^[A-Z]");
            return regex.Replace(str.ToCamelLike(), m => m.Value.ToLower());
        }

        /// <summary>
        /// 将字符串转换成 PascalCase 命名样式 <br/>
        /// 示例：NameStylesIsAGoodLibrary
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str)
        {
            Regex regex = new Regex("^[a-z]");
            return regex.Replace(str.ToCamelLike(), m => m.Value.ToUpper());
        }

        /// <summary>
        /// 将字符串转换成 snake_case 命名样式 <br/>
        /// 示例：name_styles_is_a_good_library
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string str)
        {
            return str.ToSnakeLike();
        }

        /// <summary>
        /// 将字符串转换成 kebab-case 命名样式 <br/>
        /// 示例：name-styles-is-a-good-library
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToKebabCase(this string str)
        {
            return str.ToSnakeLike().Replace("_", "-");
        }

        /// <summary>
        /// 将字符串转换成 hyphen-case 命名样式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToHyphenCase(this string str)=> str.ToKebabCase();
    }
}
