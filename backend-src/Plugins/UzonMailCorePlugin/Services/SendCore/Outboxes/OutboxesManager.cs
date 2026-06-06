using System.Collections.Concurrent;
using log4net;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Outboxes
{
    /// <summary>
    /// 发件箱管理器
    /// </summary>
    public class OutboxesManager
        : ConcurrentDictionary<string, OutboxEmailAddress>,
            ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OutboxesManager));

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
        /// 移除发件箱
        /// </summary>
        /// <param name="outbox"></param>
        public bool RemoveOutbox(OutboxEmailAddress outbox, string message)
        {
            if (!this.TryRemove(outbox.Email, out _))
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
        public List<OutboxEmailAddress> RemoveOutbox(long sendingGroupId, string message)
        {
            List<OutboxEmailAddress> removedResults = [];
            var keys = this.Keys.ToList();
            foreach (var email in keys)
            {
                if (!this.TryGetValue(email, out var outbox))
                    continue;

                outbox.RemoveSendingGroup(sendingGroupId);
                // 说明还有其它任务在使用该发件箱，不能移除
                if (outbox.IsWorking)
                    continue;

                // 移除
                this.RemoveOutbox(outbox, message);
                removedResults.Add(outbox);
            }

            return removedResults;
        }

        /// <summary>
        /// 指定发件组是否存在发件箱
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool ExistValidOutbox(long sendingGroupId)
        {
            return this
                .Values.Where(x => !x.ShouldDispose)
                .Any(x => x.ContainsSendingGroup(sendingGroupId));
        }

        /// <summary>
        /// 是否存在发作箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool ExistValidOutbox(string email)
        {
            if (!this.TryGetValue(email, out var value))
                return false;

            return !value.ShouldDispose;
        }
    }
}
