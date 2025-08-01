using System.Text.RegularExpressions;

namespace UZonMail.Core.Services.EmailDecorator
{
    public class EmailVariableHelper
    {
        /// <summary>
        /// 获取内容中的所有变量名
        /// 变量名格式为: {{变量名}}
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static List<string> GetAllVariableNames(string content)
        {
            // 获取内容中的变量
            var matches = Regex.Matches(content, @"\{\{\s*([\p{L}\p{N}_]+)\s*\}\}",RegexOptions.None);
            var variables = matches.Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();
            return variables;
        }
    }
}
