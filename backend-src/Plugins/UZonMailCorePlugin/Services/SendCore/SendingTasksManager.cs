using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore
{
    /// <summary>
    /// 发件箱任务管理器
    /// 每个发件箱对应一个任务
    /// </summary>
    public class SendingTasksManager(IServiceProvider provider, OutboxesManager outboxesManager)
        : ISendingTasksManager,
            ISingletonService<ISendingTasksManager>
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
        public void StartSending()
        {
            lock (_lockObj)
            {
                // 获取所有的邮箱组
                var outboxes = outboxesManager.Values.ToList();
                foreach (var outbox in outboxes)
                {
                    if (outbox.IsRunningInTask)
                        continue;
                    if (outbox.ShouldDispose)
                        continue;

                    // 启动任务
                    var task = Task.Run(async () =>
                    {
                        await StartSendingWorkTask(outbox);
                    });
                    outbox.SetTaskId(task.Id);
                }
            }
        }

        /// <summary>
        /// 开始任务
        /// 以发件箱的数据为索引进行发件，提高发件箱利用率
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        private async Task StartSendingWorkTask(OutboxEmailAddress outbox)
        {
            // 生成 task 的 scope
            Interlocked.Increment(ref _runningTasksCount);

            _logger.Info($"线程 {Environment.CurrentManagedThreadId} 开始执行发件任务: {outbox.Email}");
            try
            {
                while (true)
                {
                    // 在每次循环中，生成新的数据库上下文, 确保 scoped 服务（如 SqlContext）为本次迭代新实例
                    // 后期若有据性能问题，可以动态调整创建频率
                    var scope = provider.CreateAsyncScope();
                    var iterationProvider = scope.ServiceProvider;

                    // 生成服务上下文
                    var sendingContext = iterationProvider
                        .GetRequiredService<SendingContext>()
                        .SetOutbox(outbox);

                    // 创建职责链
                    // 每条职责链必定会被执行，内部需要根据状态判断是否调用核心逻辑
                    var chainHandlers = new List<Type>()
                    {
                        typeof(EmailItemGetter), // 获取邮件
                        typeof(LocalEmailSendingHandler), // 开始发件
                        typeof(EmailItemUpdateHandler), // 发件回调
                        typeof(GroupTaskUpdateHandler), // 发件任务回调
                        typeof(OutboxesUpdateHandler), // 发件箱回调
                        typeof(OutboxDisposer), // 释放发件箱
                        typeof(SmtpClientDisposer), // 释放 smtp 连接
                        typeof(OutboxSendingThrottleHandler) // 发件箱发送速度控制
                    }
                        .Select(iterationProvider.GetRequiredService)
                        .Where(x => x != null)
                        .Cast<ISendingHandler>()
                        .ToList();
                    _ = chainHandlers.Aggregate((a, b) => a.SetNext(b));
                    // 依次执行职责链
                    await chainHandlers.First().Handle(sendingContext);

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
                outbox.SetTaskId(0);
                Interlocked.Add(ref _runningTasksCount, -1);
            }
        }
    }
}
