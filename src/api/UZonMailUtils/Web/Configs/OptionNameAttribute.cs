using System;

namespace UzonMail.Utils.Web.Configs
{
    /// <summary>
    /// 指定 Options 类型对应的配置项名称。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionNameAttribute(string optionName) : Attribute
    {
        /// <summary>
        /// 配置项名称，示例：
        /// 1. AICopilot
        /// 2. AICopilot.Prompts
        /// </summary>
        public string OptionName { get; } = optionName;
    }
}
