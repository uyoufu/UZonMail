using UZonMail.DB.SQL;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 数据库缓存的基类
    /// </summary>
    /// <typeparam name="TSqlContext">数据库上下文类型</typeparam>
    public abstract class BaseDBCache<TSqlContext>: IDBCache where TSqlContext : SqlContextBase
    {
        protected bool NeedUpdate { get; private set; } = true;
        protected long LongValue { get; private set; }

        /// <summary>
        /// 设置 long 类型的 key
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetKey(string key)
        {
            LongValue = ParseLongValue(key);
        }

        /// <summary>
        /// 标记 cache 需要更新
        /// </summary>
        public void SetDirty(bool isDirty = true)
        {
            NeedUpdate = isDirty;
        }

        public abstract Task Update(TSqlContext db);
        public abstract void Dispose();

        /// <summary>
        /// 获取 long 类型的 key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static long ParseLongValue(string key)
        {
            if (!long.TryParse(key, out var longValue))
                throw new ArgumentException($"{nameof(UserInfoCache)} 需要用户 id 作为 key", nameof(key));
            if (longValue <= 0)
                throw new ArgumentException($"{nameof(key)} 需要大于 0", nameof(key));
            return longValue;
        }
    }
}
