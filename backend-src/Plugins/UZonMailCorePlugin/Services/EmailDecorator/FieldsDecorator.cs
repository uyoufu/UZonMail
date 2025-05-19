using System.Text.RegularExpressions;
using UZonMail.Core.Services.EmailDecorator.Interfaces;

namespace UZonMail.Core.Services.EmailDecorator
{
    /// <summary>
    /// 通用字段替换
    /// </summary>
    public class FieldsDecorator : IContentDecroator
    {
        // 更早执行
        public int Order { get; } = -1;

        public Task<string> StartDecorating(IContentDecoratorParams decoratorParams, string originContent)
        {
            // 将基本变量进行替换
            if (string.IsNullOrEmpty(originContent)) return Task.FromResult(originContent);

            var bodyData = decoratorParams.SendItemMeta.BodyData;

            // 替换正文变量
            if (bodyData == null) return Task.FromResult(originContent);
            foreach (var item in bodyData)
            {
                if (item.Value == null) continue;
                // 使用正则进行替换
                var regex = new Regex(@"\{\{\s*" + item.Key + @"\s*\}\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                originContent = regex.Replace(originContent, item.Value.ToString());
            }
            return Task.FromResult(originContent);
        }
    }
}
