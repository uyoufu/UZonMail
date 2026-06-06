using UZonMail.DB.SQL;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 数据库缓存的基类
    /// </summary>
    /// <typeparam name="TSqlContext">数据库上下文类型</typeparam>
    public abstract class BaseDBCache<TSqlContext, TArg> : IDBCache
        where TSqlContext : SqlContextBase
    {
        private bool _needUpdate = true;

        protected TArg Args { get; private set; }

        public int Version { get; set; } = 0;

        /// <summary>
        /// 设置 long 类型的 key
        /// </summary>
        /// <param name="arg"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetParams(TArg arg)
        {
            Args = arg;
        }

        /// <summary>
        /// 标记 cache 需要更新
        /// </summary>
        public void SetDirty()
        {
            _needUpdate = true;
            Version++;
        }

        public async Task TryUpdate(TSqlContext db)
        {
            if (!_needUpdate)
                return;

            await UpdateCore(db);

            _needUpdate = false;
        }

        protected abstract Task UpdateCore(TSqlContext db);

        public abstract void Dispose();
    }
}
