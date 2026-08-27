using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

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
        private readonly ConcurrentDictionary<long, long> _taskOrder = [];
        private readonly SemaphoreSlim _activationLock = new(1, 1);
        private long _nextTaskOrder;

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
                    await newTask.CloseAsync();
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
        /// 获取组中可被 senderAccountId 发送的邮件项
        /// </summary>
        /// <returns></returns>
        public async Task<SendItemExecution?> GetEmailItem(SendingContext context)
        {
            var candidates = _taskOrder
                .Select(x => new { GroupId = x.Key, Order = x.Value })
                .Select(x =>
                    _tasks.TryGetValue(x.GroupId, out var task)
                        ? new
                        {
                            x.GroupId,
                            x.Order,
                            Task = task
                        }
                        : null
                )
                .Where(x => x is not null)
                .Select(x => x!)
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

        public bool MatchEmailItem(SenderEmailAddress senderAccount)
        {
            if (senderAccount.UserId != UserId)
                return false;

            // 依次获取发件项
            foreach (var sendingGroupId in _taskOrder.OrderBy(x => x.Value).Select(x => x.Key))
            {
                if (!_tasks.TryGetValue(sendingGroupId, out var groupTask))
                    continue;
                var match = groupTask.MatchEmailItem(senderAccount);
                if (match)
                    return true;
            }

            return false;
        }

        public bool MatchReadyEmailItem(SenderEmailAddress senderAccount)
        {
            return senderAccount.UserId == UserId
                && _tasks.Values.Any(task => task.MatchReadyEmailItem(senderAccount));
        }

        #region 实现 ConcurrentDictionary 需要的接口
        public bool IsEmpty => _tasks.IsEmpty;

        public int Count => _tasks.Count;

        public IReadOnlyList<GroupTask> GetTasks() => [.. _tasks.Values];

        public bool TryAdd(long key, GroupTask value)
        {
            if (!_tasks.TryAdd(key, value))
                return false;

            _taskOrder[key] = Interlocked.Increment(ref _nextTaskOrder);
            return true;
        }

        public bool TryGetValue(long key, [MaybeNullWhen(false)] out GroupTask value)
        {
            return _tasks.TryGetValue(key, out value);
        }

        /// <summary>
        /// 从任务池移除发件组，并等待其异步资源释放完成。
        /// </summary>
        public async Task<bool> TryRemoveAsync(long key)
        {
            if (!_tasks.TryRemove(key, out var groupTask))
                return false;

            await groupTask.CloseAsync();
            _taskOrder.TryRemove(key, out _);
            return true;
        }
        #endregion
    }
}
