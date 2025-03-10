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
        /// 设置 long 类型的 key
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentException"></exception>
        void SetKey(string key);


        /// <summary>
        /// 标记 cache 需要更新
        /// </summary>
        void SetDirty(bool isDirty = true);

        /// <summary>
        /// 释放缓存
        /// </summary>
        abstract void Dispose();
    }
}
