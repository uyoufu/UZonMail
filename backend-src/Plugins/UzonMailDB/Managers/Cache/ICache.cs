using System.Threading.Tasks;
using UZonMail.DB.SQL;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 设置接口
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 设置 Key
        /// </summary>
        /// <param name="key"></param>
        void SetKey(string key);

        /// <summary>
        /// 初始化
        /// 对象每次从缓存中取出时，都会被调用
        /// 里面需要自行判断是否需要更新
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        Task Update(SqlContext db);

        /// <summary>
        /// 设置需要更新
        /// </summary>
        void SetDirty(bool isDirty);

        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }
}
