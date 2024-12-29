using log4net;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Interfaces;
using UZonMail.DB.SQL.Emails;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Outboxes
{
    /// <summary>
    /// 发件箱池
    /// key : 用户id，value: 发件箱池
    /// </summary>
    public class OutboxesPoolList : ConcurrentDictionary<long, OutboxesPool>, ISingletonService
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutboxesPoolList));

        /// <summary>
        /// 添加发件箱
        /// </summary>
        /// <param name="outbox"></param>
        public void AddOutbox(OutboxEmailAddress outbox)
        {
            var outboxPool = new OutboxesPool(outbox.UserId, outbox.Weight);
            if (!this.TryAdd(outbox.UserId, outboxPool))
            {
                // 若存在，获取既有的发件箱池
                outboxPool = this[outbox.UserId];
            }
            // 向发件箱池中添加发件箱
            outboxPool.AddOutbox(outbox);
        }

        /// <summary>
        /// 通过用户发件池的权重先筛选出发件池，然后从这个用户的发件池中选择一个发件箱
        /// </summary>
        /// <returns></returns>
        public OutboxEmailAddress? GetOutbox()
        {
            if (this.Count == 0)
            {
                _logger.Info("系统发件池为空");
                return null;
            }

            // 通过权重获取发件箱池
            var data = this.GetDataByWeight();

            // 未获取到发件箱
            if (data == null)
            {
                _logger.Info("发件池中暂无可用的发件箱");
                return null;
            }

            // 获取子项
            if (data is not OutboxesPool outboxesPool)
            {
                _logger.Error("获取发件箱池失败");
                return null;
            }

            var result = outboxesPool.GetOutboxByWeight();
            return result;
        }

        /// <summary>
        /// 移除发件箱
        /// </summary>
        /// <param name="outbox"></param>
        public bool RemoveOutbox(OutboxEmailAddress outbox)
        {
            if (!this.TryGetValue(outbox.UserId, out var userPool)) return true;
            return userPool.RemoveOutbox(outbox);
        }

        /// <summary>
        /// 移除组对应的发件箱
        /// </summary>
        /// <returns></returns>
        public bool RemoveOutbox(long userId, long sendingGroupId)
        {
            if (!this.TryGetValue(userId, out var userPool)) return true;
            return userPool.RemoveOutboxesBySendingGroup(sendingGroupId);
        }

        /// <summary>
        /// 指定发件组是否存在发件箱
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool ExistOutboxes(long userId, long sendingGroupId)
        {
            if(!this.TryGetValue(userId, out var userPool)) return false;
            return userPool.ExistOutboxes(sendingGroupId);
        }
    }
}
