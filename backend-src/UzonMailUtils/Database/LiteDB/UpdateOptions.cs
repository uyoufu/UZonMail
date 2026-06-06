using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Utils.Database.LiteDB
{
    public class UpdateOptions : List<string>
    {
        public UpdateOptions() { }

        public UpdateOptions(bool isExclude)
        {
            IsExclude = isExclude;
        }

        public UpdateOptions(IList<string> keys, bool isExclude = false) : base(keys)
        {
            IsExclude = isExclude;
        }

        /// <summary>
        /// 是否不包含在集合中
        /// true：不在集合中的字段才更新
        /// </summary>
        public bool IsExclude { get; set; }

        /// <summary>
        /// 忽略大小写
        /// </summary>
        public bool IgnoreCase { get; set; } = true;

        public bool Validate(string value)
        {
            if (IgnoreCase)
            {
                var valueTemp = value.ToLower();
                bool isIncluding = this.Any(x => x.ToLower() == valueTemp);
                if (IsExclude) return !isIncluding;
                return isIncluding;
            }
            else
            {
                if (IsExclude) return !Contains(value);
                return Contains(value);
            }
        }

        /// <summary>
        /// 忽略默认值，返回有值的字段名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static UpdateOptions CreateIgnoreDefaultValue<T>(T data)
        {
            Type dataType = data.GetType();
            var properties = dataType.GetProperties();
            List<string> keys = new List<string>();

            foreach (var prop in properties)
            {
                object value = prop.GetValue(data);
                if (value == null) continue;

                if (value is string && string.IsNullOrEmpty(value.ToString())) continue;
                if (value is ICollection collection && collection.Count == 0) continue;
                if (value.GetType().IsEnum)
                {
                    var enumValue = (int)value;
                    if (enumValue == 0) continue;
                }


                keys.Add(prop.Name);
            }

            return new UpdateOptions(keys);
        }
    }
}
