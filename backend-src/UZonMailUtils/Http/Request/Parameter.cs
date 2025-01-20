using System;

namespace UZonMail.Utils.Http.Request
{
    public class Parameter(string name, string value, ParameterType type)
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = value;

        /// <summary>
        /// 类型
        /// </summary>
        public ParameterType Type { get; set; } = type;

        public string Key => $"{Type}:{Name}";

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Parameter parameter)
            {
                return false;
            }
            return parameter.Key == parameter.Key;
        }
    }
}
