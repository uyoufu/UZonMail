using System.Threading.Tasks;
using UZonMail.DB.SQL;

namespace UZonMail.DB.Managers.Cache
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
