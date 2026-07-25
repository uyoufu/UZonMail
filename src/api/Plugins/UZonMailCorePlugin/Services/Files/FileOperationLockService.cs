using System.Collections.Concurrent;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 为同一用户或同一内容哈希的并发文件操作提供进程内异步互斥。
    /// </summary>
    public sealed class FileOperationLockService : ISingletonService
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        /// <summary>
        /// 获取指定业务键的异步锁，释放返回对象即可退出临界区。
        /// </summary>
        public async Task<IDisposable> AcquireAsync(
            string operationKey,
            CancellationToken cancellationToken = default
        )
        {
            var semaphore = _locks.GetOrAdd(operationKey, static _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(cancellationToken);
            return new Releaser(semaphore);
        }

        private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
        {
            private bool _isDisposed;

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                semaphore.Release();
            }
        }
    }
}
