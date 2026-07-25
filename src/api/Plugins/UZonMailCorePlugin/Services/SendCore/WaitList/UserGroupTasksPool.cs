using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList
{
    /// <summary>
    /// 单个用户的发件任务池
    /// 先添加的任务先发送
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupFairShare">单组公平份额，不是硬上限。</param>
    public class UserGroupTasksPool(long userId, int groupFairShare)
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserGroupTasksPool));
        private readonly ConcurrentDictionary<long, GroupTask> _tasks = [];
        private readonly ConcurrentQueue<long> _taskOrder = [];
        private readonly SemaphoreSlim _activationLock = new(1, 1);

        /// <summary>
        /// 用户 id
        /// </summary>
        public long UserId { get; set; } = userId;

        /// <summary>
        /// 添加发件组任务
        /// 若包含 sendingItemIds，则只发送这部分邮件
        /// </summary>
        /// <param name="scopeServices"></param>
        /// <param name="sendingGroupId">传入时请保证组一定存在</param>
        /// <param name="smtpPasswordSecretKeys">smtp密码密钥</param>
        /// <param name="sendingItemIds">待发送的 Id</param>
        /// <returns></returns>
        public async Task<bool> AddSendingGroup(
            SendingContext scopeServices,
            long sendingGroupId,
            List<long>? sendingItemIds = null
        )
        {
            await _activationLock.WaitAsync();
            try
            {
                if (TryGetValue(sendingGroupId, out var existTask))
                    return await existTask.InitSendingItems(scopeServices, sendingItemIds);

                var newTask = await GroupTask.Create(scopeServices, sendingGroupId);
                if (newTask == null)
                    return false;

                var success = await newTask.InitSendingItems(scopeServices, sendingItemIds);
                if (!success || !TryAdd(sendingGroupId, newTask))
                {
                    newTask.Close();
                    return false;
                }
                return true;
            }
            finally
            {
                _activationLock.Release();
            }
        }

        /// <summary>
        /// 获取组中可被 outboxId 发送的邮件项
        /// </summary>
        /// <returns></returns>
        public async Task<SendItemExecution?> GetEmailItem(SendingContext context)
        {
            var candidates = _taskOrder
                .Select((groupId, order) => new { GroupId = groupId, Order = order })
                .Where(x => _tasks.ContainsKey(x.GroupId))
                .Select(x => new
                {
                    x.GroupId,
                    x.Order,
                    Task = _tasks[x.GroupId],
                })
                .OrderBy(x => x.Task.ActiveCount >= groupFairShare)
                .ThenBy(x => x.Task.ActiveCount)
                .ThenBy(x => x.Order)
                .ToList();

            foreach (var candidate in candidates)
            {
                var result = await candidate.Task.GetEmailItem(context);
                if (result != null)
                    return result;
            }

            return null;
        }

        public bool MatchEmailItem(OutboxEmailAddress outbox)
        {
            if (outbox.UserId != UserId)
                return false;

            // 依次获取发件项
            foreach (var sendingGroupId in _taskOrder)
            {
                if (!_tasks.TryGetValue(sendingGroupId, out var groupTask))
                    continue;
                var match = groupTask.MatchEmailItem(outbox);
                if (match)
                    return true;
            }

            return false;
        }

        public bool MatchReadyEmailItem(OutboxEmailAddress outbox)
        {
            return outbox.UserId == UserId
                && _tasks.Values.Any(task => task.MatchReadyEmailItem(outbox));
        }

        #region 实现 ConcurrentDictionary 需要的接口
        public bool IsEmpty => _tasks.IsEmpty;

        public int Count => _tasks.Count;

        public IReadOnlyList<GroupTask> GetTasks() => [.. _tasks.Values];

        public bool TryAdd(long key, GroupTask value)
        {
            if (!_tasks.TryAdd(key, value))
                return false;

            _taskOrder.Enqueue(key);
            return true;
        }

        public bool TryGetValue(long key, [MaybeNullWhen(false)] out GroupTask value)
        {
            return _tasks.TryGetValue(key, out value);
        }

        public bool TryRemove(long key, [MaybeNullWhen(false)] out GroupTask value)
        {
            if (!_tasks.TryRemove(key, out value))
                return false;

            value.Close();

            return true;
        }
        #endregion
    }
}
