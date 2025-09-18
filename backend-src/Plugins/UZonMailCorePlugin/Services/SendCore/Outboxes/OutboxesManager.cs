using log4net;
using System.Collections.Concurrent;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Outboxes
{
    public class OutboxesManager : ConcurrentDictionary<string, OutboxEmailAddress>, ISingletonService
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutboxesManager));

        /// <summary>
        /// 添加发件箱
        /// </summary>
        /// <param name="outbox"></param>
        public void AddOutbox(OutboxEmailAddress outbox)
        {
            if (this.TryGetValue(outbox.Email, out var existValue))
            {
                existValue.Update(outbox);
                return;
            }
            // 新增
            this.TryAdd(outbox.Email, outbox);
        }

        /// <summary>
        /// 通过用户发件池的权重先筛选出发件池，然后从这个用户的发件池中选择一个发件箱
        /// </summary>
        /// <returns></returns>
        [Obsolete("后期将移除")]
        public OutboxEmailAddress? GetOutbox()
        {
            // 随机返回一个值
            var randIndex = new Random().Next(0, this.Count);
            return this.Values.ElementAt(randIndex);
        }

        /// <summary>
        /// 移除发件箱
        /// </summary>
        /// <param name="outbox"></param>
        public bool RemoveOutbox(OutboxEmailAddress outbox, string message = "系统检测到发件箱不可用或取消，主动释放")
        {
            if (!this.TryRemove(outbox.Email, out var existValue))
            {
                return false;
            }

            // 更新状态
            outbox.MarkShouldDispose(message);
            return true;
        }

        /// <summary>
        /// 移除组对应的发件箱
        /// </summary>
        /// <returns></returns>
        public List<OutboxEmailAddress> RemoveOutbox(long sendingGroupId)
        {
            List<OutboxEmailAddress> removedResults = [];
            var keys = this.Keys.ToList();
            foreach (var email in keys)
            {
                if (!this.TryGetValue(email, out var outbox)) continue;

                outbox.RemoveSendingGroup(sendingGroupId);
                // 说明还有其它任务在使用该发件箱，不能移除
                if (outbox.IsWorking) continue;

                // 移除
                this.RemoveOutbox(outbox);
                removedResults.Add(outbox);
            }

            return removedResults;
        }

        /// <summary>
        /// 指定发件组是否存在发件箱
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool ExistOutboxes(long sendingGroupId)
        {
            return this.Values.Any(x => x.ContainsSendingGroup(sendingGroupId));
        }

        /// <summary>
        /// 是否存在发作箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool ExistOutbox(string email)
        {
            return this.ContainsKey(email);
        }
    }
}
