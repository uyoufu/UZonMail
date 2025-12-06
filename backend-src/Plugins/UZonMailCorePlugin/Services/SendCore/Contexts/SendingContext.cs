using Microsoft.AspNetCore.SignalR;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.SignalRHubs;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Contexts
{
    public class SendingContext(IServiceProvider provider) : ITransientService
    {
        public IServiceProvider Provider => provider;

        /// <summary>
        /// 上下文状态
        /// </summary>
        public ContextStatus Status { get; set; }

        /// <summary>
        /// 获取 SignalR 客户端
        /// </summary>
        public IHubContext<UzonMailHub, IUzonMailClient> HubClient =>
            Provider.GetRequiredService<IHubContext<UzonMailHub, IUzonMailClient>>();

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public SqlContext SqlContext => Provider.GetRequiredService<SqlContext>();

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
        #endregion
    }
}
