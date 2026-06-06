using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Utils.Dictionary
{
    /// <summary>
    /// 特性
    /// 当有重复键值时，可设置忽略或者覆盖
    /// 支持多个键值对
    /// </summary>
    public class DictionaryPlus<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
    {
        private readonly DictionaryPlusSetting _setting;
        public DictionaryPlus(DictionaryPlusSetting setting)
        {
            _setting = setting;
        }

        public DictionaryPlus() : this(DictionaryPlusSetting.IgnoreWhenSameKey)
        {
        }

        /// <summary>
        /// 覆盖父类 add 方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                // 根据设置来处理
                switch (_setting)
                {
                    case DictionaryPlusSetting.IgnoreWhenSameKey:
                        break;
                    case DictionaryPlusSetting.ReplaceWhenSameKey:
                        this[key] = value;
                        break;
                    default:
                        throw new ArgumentNullException($"存在相同键{key}");
                }

                return;
            }

            // 不存在时，直接新增
            base.Add(key, value);
        }


        private List<List<TKey>> _groupKeys = new List<List<TKey>>();
        /// <summary>
        /// 可以添加多个键
        /// </summary>
        /// <param name="keys"></param>
        /// <param name=""></param>
        public void Add(List<TKey> keys, TValue value)
        {
            // 保存分组关系
            _groupKeys.Add(keys);

            keys.ForEach(x => Add(x, value));
        }

        public new void Remove(TKey key)
        {
            // 判断是否在组中，如果是一个组，则要同时删除
            var group = _groupKeys.Find(x => x.Contains(key));
            if (group != null)
            {
                group.ForEach(x => base.Remove(x));
                return;
            }

            // 仅移除当前 key
            base.Remove(key);
        }
    }

    /// <summary>
    /// 设置
    /// </summary>
    [Flags]
    public enum DictionaryPlusSetting
    {
        /// <summary>
        /// 当同名键时忽略
        /// </summary>
        IgnoreWhenSameKey = 1,


        /// <summary>
        /// 当同名键时替换
        /// </summary>
        ReplaceWhenSameKey = 2,

        /// <summary>
        /// 当同名键时忽略抛出异常
        /// </summary>
        ThrowErrorWhenSameKey = 4,
    }
}
