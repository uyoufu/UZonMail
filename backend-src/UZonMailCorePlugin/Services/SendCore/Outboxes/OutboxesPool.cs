using log4net;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Interfaces;

namespace UZonMail.Core.Services.SendCore.Outboxes
{
    /// <summary>
    /// 单个用户的发件箱池
    /// 每个邮箱账号共用冷却池
    /// key: 邮箱号 ，value: 发件箱列表
    /// </summary>
    /// <param name="userId">所属用户id</param>
    /// <param name="weight">权重</param>
    public class OutboxesPool(long userId, int weight) : ConcurrentDictionary<string, OutboxEmailAddress>, IWeight
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutboxesPool));

        #region 自定义参数
        public long UserId { get; } = userId;
        #endregion

        #region 接口实现
        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; private set; } = weight > 0 ? weight : 1;

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Enable
        {
            get
            {
                if (this.Count == 0) return false;
                return this.Values.Any(x => x.Enable);
            }
        }
        #endregion

        /// <summary>
        /// 添加发件箱
        /// </summary>
        /// <param name="outbox"></param>
        public bool AddOutbox(OutboxEmailAddress outbox)
        {
            if (this.TryGetValue(outbox.Email, out var existValue))
            {
                existValue.Update(outbox);
                return true;
            }

            // 不存在则添加
            return this.TryAdd(outbox.Email, outbox);
        }

        /// <summary>
        /// 移除发件箱
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public bool RemoveOutbox(OutboxEmailAddress outbox)
        {
            return this.TryRemove(outbox.Email, out _);
        }

        /// <summary>
        /// 移除发件组关联的 outboxes
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool RemoveOutboxesBySendingGroup(long sendingGroupId)
        {
            var outboxes = this.Values;
            foreach (var outbox in outboxes)
            {
                outbox.RemoveSendingGroup(sendingGroupId);
                if (outbox.Working) continue;

                // 移除
                this.TryRemove(outbox.Email, out _);
            }

            return true;
        }

        /// <summary>
        /// 按用户设置的权重获取发件箱
        /// </summary>
        /// <returns></returns>
        public OutboxEmailAddress? GetOutboxByWeight()
        {
            var data = this.GetDataByWeight();
            if (data is not OutboxEmailAddress outbox)
            {
                _logger.Info($"未能从{UserId}池中获取发件箱");
                return null;
            }

            if (!outbox.LockUsing())
            {
                _logger.Info($"发件箱 {outbox.Email} 已被其它线程使用，锁定失败");
                return null;
            }

            // 保存当前引用
            return outbox;
        }


        /// <summary>
        /// 给定发件组是否存在发件箱
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool ExistOutboxes(long sendingGroupId)
        {
            return this.Values.Any(x => !x.ShouldDispose && x.ContainsSendingGroup(sendingGroupId));
        }
    }
}
