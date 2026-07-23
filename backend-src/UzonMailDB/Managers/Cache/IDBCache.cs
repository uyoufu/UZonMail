using System.Threading.Tasks;
using UzonMail.DB.SQL;

namespace UzonMail.DB.Managers.Cache
{
    /// <summary>
    /// 设置接口
    /// </summary>
    public interface IDBCache
    {
        /// <summary>
        /// 标记 cache 需要更新
        /// </summary>
        void SetDirty();

        /// <summary>
        /// 释放缓存
        /// </summary>
        abstract void Dispose();
    }
}
