using log4net;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    /// <summary>
    /// 发件箱任务管理器
    /// 每个发件箱对应一个任务
    /// </summary>
    public class SendingTasksManager(IServiceProvider provider, OutboxesManager outboxesManager)
        : ISendingTasksManager,
            ISendingWorkerCoordinator,
            ISingletonService<ISendingTasksManager>,
            ISingletonService<ISendingWorkerCoordinator>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SendingTasksManager));
        private readonly Lock _lockObj = new();

        /// <summary>
        /// 发件任务数量
        /// </summary>
        private int _runningTasksCount = 0;
        public int RunningTasksCount => _runningTasksCount;

        /// <summary>
        /// 启动发件任务
        /// </summary>
        public Task StartSendingAsync(CancellationToken cancellationToken = default)
        {
            lock (_lockObj)
            {
                // 获取所有的邮箱组
                var outboxes = outboxesManager.Values.ToList();
                foreach (var outbox in outboxes)
                {
                    if (outbox.ShouldDispose)
                        continue;
                    if (!outbox.TryMarkTaskRunning())
                        continue;

                    // 启动任务
                    _ = Task.Run(async () =>
                    {
                        await StartSendingWorkTask(outbox, cancellationToken);
                    });
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 开始任务
        /// 以发件箱的数据为索引进行发件，提高发件箱利用率
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        private async Task StartSendingWorkTask(
            OutboxEmailAddress outbox,
            CancellationToken cancellationToken
        )
        {
            // 生成 task 的 scope
            Interlocked.Increment(ref _runningTasksCount);

            _logger.Info($"线程 {Environment.CurrentManagedThreadId} 开始执行发件任务: {outbox.Email}");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 在每次循环中，生成新的数据库上下文, 确保 scoped 服务（如 SqlContext）为本次迭代新实例
                    // 后期若有据性能问题，可以动态调整创建频率
                    await using var scope = provider.CreateAsyncScope();
                    var iterationProvider = scope.ServiceProvider;

                    var sendingContext = iterationProvider
                        .GetRequiredService<Contexts.SendingContext>()
                        .SetOutbox(outbox);

                    var pipeline = iterationProvider.GetRequiredService<ISendingPipeline>();
                    await pipeline.Handle(sendingContext);

                    // 根据返回值，判断线程是否需要继续
                    if (sendingContext.ShouldExitTask())
                    {
                        _logger.Info(
                            $"发件任务执行异常，线程 {Environment.CurrentManagedThreadId} 结束 {outbox.Email} 发件任务"
                        );
                        break;
                    }

                    // 若发件箱被标记为释放，则结束任务
                    if (outbox.ShouldDispose)
                    {
                        _logger.Info(
                            $"发件箱已标记为释放，线程 {Environment.CurrentManagedThreadId} 结束 {outbox.Email} 发件任务"
                        );
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"发件箱 {outbox.Email} 发件任务异常终止: {ex.Message}", ex);
            }
            finally
            {
                // 结束后，清除任务 ID
                outbox.MarkTaskStopped();
                Interlocked.Add(ref _runningTasksCount, -1);
            }
        }
    }
}
