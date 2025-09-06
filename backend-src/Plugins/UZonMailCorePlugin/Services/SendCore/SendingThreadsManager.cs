using log4net;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Interfaces;
using UZonMail.Core.Services.SendCore.ResponsibilityChains;
using UZonMail.Utils.Web.Service;
using Timer = System.Timers.Timer;

namespace UZonMail.Core.Services.SendCore
{
    public class SendingThreadsManager : ISendingThreadsManager, ISingletonService<ISendingThreadsManager>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SendingThreadsManager));
        private IServiceScopeFactory _ssf;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="waitList"></param>
        /// <param name="outboxesPool"></param>
        public SendingThreadsManager(IServiceScopeFactory ssf)
        {
            this._ssf = ssf;

            // 自动激活，防止特殊情况被锁死
            SetAutoActive();
        }

        /// <summary>
        /// 发件任务
        /// </summary>
        private int _runningTasksCount = 0;
        public int RunningTasksCount => _runningTasksCount;

        #region 外部调用的方法
        /// <summary>
        /// 外部调用该方法开始发件
        /// 每次调用会打开指定数量的线程
        /// </summary>
        /// <param name="activeCount">若小于等于 0，则全部激活</param>
        public void StartSending(int activeCount = 0)
        {
            // 获取核心数
            int coreCount = Environment.ProcessorCount;
            // 任务数 = 2*核心数
            int maxTasksCount = 2 * coreCount;

            int needCount = 0;
            if (activeCount <= 0)
            {
                // 创建全部最大任务数量
                needCount = maxTasksCount - _runningTasksCount;
            }
            else
            {
                needCount = Math.Min(activeCount, maxTasksCount - _runningTasksCount);
            }

            // 开始创建任务
            for (int i = 0; i < needCount; i++)
            {
                var task = new Task(async () =>
                {
                    // 任务开始
                    await DoSendingWork();
                });
                task.Start();
            }
        }

        private Timer? _timer;
        /// <summary>
        /// 每隔 1 分钟激活一次，防止因特殊原因使发件任务被锁死
        /// </summary>
        private void SetAutoActive()
        {
            if (_timer == null)
            {
                _timer = new Timer(60000)
                {
                    AutoReset = true
                };

                _timer.Elapsed += (sender, e) =>
                {
                    _logger.Info("守护任务开始执行 ...");
                    StartSending(1);
                };
                _timer.Start();
            }
        }

        /// <summary>
        /// 开始任务
        /// 以发件箱的数据为索引进行发件，提高发件箱利用率
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        private async Task DoSendingWork()
        {
            // 生成 task 的 scope
            var scope = _ssf.CreateAsyncScope();
            Interlocked.Increment(ref _runningTasksCount);

            _logger.Info($"线程 {Environment.CurrentManagedThreadId} 开始工作 ...");
            while (true)
            {
                var provider = scope.ServiceProvider;
                // 生成服务
                var sendingContext = provider.GetRequiredService<SendingContext>();

                // 创建职责链
                // 每条职责链必定会被执行，内部需要根据状态判断是否调用核心逻辑
                var chainHandlers = new List<Type>()
                {
                    typeof(OutboxGetter), // 获取发件箱
                    typeof(EmailItemGetter), // 获取邮件
                    typeof(LocalEmailSendingHandler), // 开始发件
                    typeof(EmailItemPostHandler), // 发件回调
                    typeof(GroupTaskPostHandler), // 发件任务回调
                    typeof(OutboxesPostHandler), // 发件箱回调
                    typeof(OutboxCooler), // 发件箱冷却重置
                    typeof(OutboxUnlocker), // 发件箱解锁
                    typeof(SmtpClientDisposer) // 释放 smtp 连接
                } 
                .Select(provider.GetRequiredService)
                .Where(x => x != null)
                .Cast<ISendingHandler>()
                .ToList();
                _ = chainHandlers.Aggregate((a, b) => a.SetNext(b));
                await chainHandlers.First().Handle(sendingContext);

                // 根据返回值，判断线程是否需要继续
                if (sendingContext.Status.HasFlag(ContextStatus.ShouldExitTask))
                {
                    _logger.Info($"线程 {Environment.CurrentManagedThreadId} 结束工作 ...");
                    break;
                }
            }

            // 释放资源
            await scope.DisposeAsync();
            Interlocked.Add(ref _runningTasksCount, -1);
        }
        #endregion
    }
}
