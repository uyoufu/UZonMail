using System.Text.RegularExpressions;
using UZonMail.CorePlugin.Services.EmailDecorator.Interfaces;

namespace UZonMail.CorePlugin.Services.EmailDecorator
{
    /// <summary>
    /// 通用字段替换
    /// </summary>
    public class FieldsDecorator : IContentDecroator, IVariableResolver
    {
        // 更早执行
        public int Order { get; } = -1;

        public Task<string> StartDecorating(
            IContentDecoratorParams decoratorParams,
            string originContent
        )
        {
            // 将基本变量进行替换
            if (string.IsNullOrEmpty(originContent))
                return Task.FromResult(originContent);

            var bodyData = decoratorParams.SendItemMeta.BodyData;
            // 替换正文变量
            if (bodyData == null)
                return Task.FromResult(originContent);

            // 获取内容中的变量
            var variables = EmailVariableHelper.GetAllVariableContents(originContent);
            if (variables.Count == 0)
                return Task.FromResult(originContent);

            Dictionary<string, string> emailItemVariables =
                new()
                {
                    { "inbox", decoratorParams.SendItemMeta.Inboxes.First().Email },
                    { "inboxName", decoratorParams.SendItemMeta.Inboxes.First().Name ?? "" },
                    { "outbox", decoratorParams.Outbox.Email },
                    { "outboxName", decoratorParams.Outbox.Name ?? "" }
                };

            foreach (var variable in variables)
            {
                string? value = null;
                // 先从数据中获取值
                // 再从邮件变量中获取值
                if (bodyData.ContainsKey(variable))
                {
                    value = bodyData[variable]!.ToString();
                }
                else if (!emailItemVariables.TryGetValue(variable, out value))
                {
                    continue;
                }

                // 判断是否存在值
                if (value == null)
                    continue;

                // 使用正则进行替换
                var regex = new Regex(
                    @"\{\{\s*" + variable + @"\s*\}\}",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline
                );
                originContent = regex.Replace(originContent, value.ToString());
            }

            return Task.FromResult(originContent);
        }
    }
}
