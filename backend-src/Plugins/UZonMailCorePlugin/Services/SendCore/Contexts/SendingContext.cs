using Microsoft.AspNetCore.SignalR;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.SignalRHubs;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Contexts
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
        public IHubContext<UzonMailHub, IUzonMailClient> HubClient => Provider.GetRequiredService<IHubContext<UzonMailHub, IUzonMailClient>>();

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
        public OutboxEmailAddress? OutboxAddress { get; set; }

        #region 发件列表相关临时参数
        /// <summary>
        /// 发件项
        /// </summary>
        public SendItemMeta? EmailItem { get; set; }
        #endregion
        #endregion
    }
}
