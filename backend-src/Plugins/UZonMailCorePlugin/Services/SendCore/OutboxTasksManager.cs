using log4net;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Interfaces;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.SendCore.ResponsibilityChains;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore
{
    /// <summary>
    /// 发件箱任务管理器
    /// </summary>
    public class OutboxTasksManager(IServiceProvider provider, OutboxesManager outboxesManager) : ISendingTasksManager, ISingletonService<ISendingTasksManager>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(OutboxTasksManager));
        private readonly object _lockObj = new();

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
                    if (outbox.IsRunningInTask) continue;
                    if (outbox.ShouldDispose) continue;

                    // 启动任务
                    var task = Task.Run(async () =>
                    {
                        await StartSendingWorkTask(outbox);

                        // 结束后，清理数据残留
                        outbox.SetTaskId(0);
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
            var scope = provider.CreateAsyncScope();
            Interlocked.Increment(ref _runningTasksCount);

            _logger.Info($"线程 {Environment.CurrentManagedThreadId} 开始执行发件任务: {outbox.Email}");
            while (true)
            {
                var provider = scope.ServiceProvider;
                // 生成服务上下文
                var sendingContext = provider.GetRequiredService<SendingContext>()
                    .SetOutbox(outbox);

                // 创建职责链
                // 每条职责链必定会被执行，内部需要根据状态判断是否调用核心逻辑
                var chainHandlers = new List<Type>()
                {
                    typeof(EmailItemGetter), // 获取邮件
                    typeof(LocalEmailSendingHandler), // 开始发件
                    typeof(EmailItemPostHandler), // 发件回调
                    typeof(GroupTaskPostHandler), // 发件任务回调
                    typeof(OutboxesPostHandler), // 发件箱回调                    
                    typeof(SmtpClientDisposer), // 释放 smtp 连接
                    typeof(OutboxSendingSpeedController) // 发件箱发送速度控制
                }
                .Select(provider.GetRequiredService)
                .Where(x => x != null)
                .Cast<ISendingHandler>()
                .ToList();
                _ = chainHandlers.Aggregate((a, b) => a.SetNext(b));
                // 依次执行职责链
                await chainHandlers.First().Handle(sendingContext);

                // 根据返回值，判断线程是否需要继续
                if (sendingContext.Status.HasFlag(ContextStatus.ShouldExitTask))
                {
                    _logger.Info($"发件任务执行异常，线程 {Environment.CurrentManagedThreadId} 结束 {outbox.Email} 发件任务");
                    break;
                }

                // 若发件箱被标记为释放，则结束任务
                if (outbox.ShouldDispose)
                {
                    _logger.Info($"发件箱已标记为释放，线程 {Environment.CurrentManagedThreadId} 结束 {outbox.Email} 发件任务");
                    break;
                }
            }

            // 释放资源
            await scope.DisposeAsync();
            Interlocked.Add(ref _runningTasksCount, -1);
        }
    }
}
