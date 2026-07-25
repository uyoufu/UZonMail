using Microsoft.AspNetCore.SignalR;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.DB.SQL;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Contexts
{
    /// <summary>
    /// 服务上下文
    /// 任何地方不要保存此对象引用，避免跨请求污染数据
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="hubClient"></param>
    /// <param name="sqlContext"></param>
    public class SendingContext(
        IServiceProvider provider,
        IHubContext<UzonMailHub, IUzonMailClient> hubClient,
        SqlContext sqlContext
    ) : IScopedService
    {
        public List<IHandlerResult> HandleResults { get; private set; } = [HandlerResult.Normal()];

        public IServiceProvider Provider => provider;

        /// <summary>
        /// 获取 SignalR 客户端
        /// </summary>
        public IHubContext<UzonMailHub, IUzonMailClient> HubClient => hubClient;

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public SqlContext SqlContext => sqlContext;

        #region 中间变量
        /// <summary>
        /// 发件任务开始时间
        /// </summary>
        public DateTime GroupTaskStartDate { get; set; }

        /// <summary>
        /// 发件箱地址
        /// </summary>
        public OutboxEmailAddress? OutboxAddress { get; private set; }

        #region 发件列表相关临时参数
        /// <summary>
        /// 当前已取得租约并完成载荷准备的发送尝试。
        /// </summary>
        public SendItemExecution? CurrentAttempt { get; set; }

        /// <summary>当前发件项所属的组任务。</summary>
        public GroupTask? GroupTask { get; set; }

        /// <summary>传输层返回的原始结果。</summary>
        public TransportResult? TransportResult { get; set; }

        /// <summary>原始结果经过业务规则分类后的提交决策。</summary>
        public SendAttemptDecision? SendAttemptDecision { get; set; }

        /// <summary>当前发件箱退出运行池后的处理结果。</summary>
        public OutboxRetirementResult? OutboxRetirement { get; set; }

        private bool ExitWorkerRequested { get; set; }
        #endregion

        #endregion

        #region 外部调用的方法
        public SendingContext SetOutbox(OutboxEmailAddress outbox)
        {
            this.OutboxAddress = outbox;
            return this;
        }

        /// <summary>
        /// 是否有失败的处理器
        /// </summary>
        /// <returns></returns>
        public bool IsFailed()
        {
            return HandleResults.Any(result => result.HandlerStatus == HandlerStatus.Failed);
        }

        /// <summary>
        /// 是否有需要退出任务的处理器
        /// </summary>
        /// <returns></returns>
        public bool ShouldExitTask()
        {
            return ExitWorkerRequested
                || HandleResults.Any(result => result.ChainStatus == ChainStatus.ShouldExitTask);
        }

        /// <summary>要求当前发送 Worker 在本轮管线结束后退出。</summary>
        public void RequestWorkerExit()
        {
            ExitWorkerRequested = true;
        }
        #endregion
    }
}
