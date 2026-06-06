using System.Diagnostics;

namespace UZonMail.CorePlugin.Services.SendCore.Utils
{
    /// <summary>
    /// 使用原子操作实现的锁
    /// 参考：https://blog.exsvc.cn/article/csharp-concurrent-lock-performance.html
    /// </summary>
    public class Locker
    {
        // 0 表示未锁定，1 表示已锁定
        private int _lockSign = 0;

        /// <summary>
        /// 是否已锁定
        /// </summary>
        public bool IsLocked => _lockSign == 1;

        /// <summary>
        /// 开始锁定
        /// </summary>
        /// <returns></returns>
        public bool Lock()
        {
            return Interlocked.CompareExchange(ref _lockSign, 1, 0) == 0;
        }

        /// <summary>
        /// 取消锁定
        /// </summary>
        public void Unlock()
        {
            Interlocked.CompareExchange(ref _lockSign, 0, 1);
        }
    }
}
