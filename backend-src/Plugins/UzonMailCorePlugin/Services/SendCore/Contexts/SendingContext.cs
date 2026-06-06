using Microsoft.AspNetCore.SignalR;
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
        /// 发件项
        /// </summary>
        public SendItemMeta? EmailItem { get; set; }
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
            return HandleResults.Any(result => result.ChainStatus == ChainStatus.ShouldExitTask);
        }
        #endregion
    }
}
