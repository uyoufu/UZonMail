using log4net;
using UZonMail.Core.Services.EmailSending.OutboxPool;
using UZonMail.Core.Services.EmailSending.Pipeline;
using UZonMail.Core.Services.SendCore.Event;
using UZonMail.Core.Services.SendCore.Event.Commands;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Utils.Web.Service;
using Timer = System.Timers.Timer;

namespace UZonMail.Core.Services.EmailSending.Sender
{
    /// <summary>
    /// 系统级发件中心
    /// </summary>
    public class SendingThreadManager : ISingletonService
    {
        private ILog _logger = LogManager.GetLogger(typeof(SendingThreadManager));

        private IServiceScopeFactory ssf;
        private GroupTasksList waitList;
        private UserOutboxesPoolManager outboxesPool;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="waitList"></param>
        /// <param name="outboxesPool"></param>
        public SendingThreadManager(
              IServiceScopeFactory ssf
            , GroupTasksList waitList
            , UserOutboxesPoolManager outboxesPool)
        {
            this.ssf = ssf;
            this.waitList = waitList;
            this.outboxesPool = outboxesPool;

            EventCenter.Core.DataChanged += EventCenter_DataChanged;

            // 自动激活，防止特殊情况被锁死
            SetAutoActive();
        }

        private async Task EventCenter_DataChanged(object? arg1, CommandBase arg2)
        {
            switch (arg2.CommandType)
            {
                case CommandType.StartSending:
                    {
                        if (arg2 is not StartSendingCommand startSendingCommand)
                            break;
                        StartSending(startSendingCommand.Data);
                        break;
                    }
                default:
                    break;
            }
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
            // 保证数据库查询期间有其它任务处理任务
            int maxTasksCount = 2;

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
                    await DoWork();
                });
                task.Start();
            }
        }

        private Timer? _timer;
        /// <summary>
        /// 每隔 1 分钟激活一次，防止因特殊原因使发件任务被锁死
        /// </summary>
        public void SetAutoActive()
        {
            if (_timer == null)
            {
                _timer = new Timer(60000)
                {
                    AutoReset = true
                };

                _timer.Elapsed += (sender, e) =>
                {
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
        private async Task DoWork()
        {
            // 保存进程 Id
            ThreadContext.Properties["threadId"] = Environment.CurrentManagedThreadId;
            // 生成 task 的 scope
            var scope = ssf.CreateAsyncScope();
            Interlocked.Increment(ref _runningTasksCount);

            // 当线程没有取消时
            while (true)
            {
                // 生成服务
                var sendingContext = new SendingContext(scope.ServiceProvider);
                try
                {
                    // 取出发件箱
                    // 并解析发件箱的代理信息
                    // 发件箱出队必须保证线程安全
                    var outboxResult = await outboxesPool.GetOutboxByWeight(sendingContext);
                    if (outboxResult.NotOk)
                    {
                        _logger.Info(outboxResult.Message ?? $"所有发件箱处于冷却中, 线程 [{Environment.CurrentManagedThreadId}] 即将退出");
                        break;
                    }

                    var outbox = outboxResult.Data;
                    _logger.Info($"线程 [{Environment.CurrentManagedThreadId}] 开始使用 {outbox.Email} 发件");
                    // 取出该发件箱对应的邮件数据
                    var sendItem = await waitList.GetSendItem(sendingContext, outbox);
                    if (sendItem == null)
                    {
                        continue;
                    }

                    // 发送邮件
                    var sendMethod = sendItem.ToSendMethod();
                    await sendMethod.Send(sendingContext);
                }
                finally
                {
                    await sendingContext.DisposeAsync();
                }
            }
            await scope.DisposeAsync();
            Interlocked.Add(ref _runningTasksCount, -1);
        }
        #endregion
    }
}
