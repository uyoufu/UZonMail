using System.Collections.Concurrent;
using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.WaitList
{
    /// <summary>
    /// 任务单例缓存数据
    /// </summary>
    public class UserGroupTasksPools
        : ConcurrentDictionary<long, UserGroupTasksPool>,
            ISingletonService { }

    /// <summary>
    /// 系统级的待发件调度器
    /// </summary>
    public class GroupTasksManager(UserGroupTasksPools userTasksPools, SqlContext sqlContext)
        : IScopedService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GroupTasksManager));

        /// <summary>
        /// 将发件组添加到待发件队列
        /// 内部会自动向前端发送消息通知
        /// 内部已调用 CreateSendingGroup
        /// </summary>
        /// <param name="sendingContext">数据库上文</param>
        /// <param name="group"></param>
        /// <param name="sendingItemIds"></param>
        /// <returns></returns>
        public async Task<bool> AddSendingGroup(
            SendingContext sendingContext,
            SendingGroup group,
            List<long>? sendingItemIds = null
        )
        {
            if (group == null)
                return false;

            // 判断是否有用户发件管理器
            if (!userTasksPools.TryGetValue(group.UserId, out var groupTasks))
            {
                // 新建用户发件管理器
                groupTasks = new UserGroupTasksPool(group.UserId);
                userTasksPools.TryAdd(group.UserId, groupTasks);
            }

            // 向发件管理器添加发件组
            bool result = await groupTasks.AddSendingGroup(
                sendingContext,
                group.Id,
                sendingItemIds
            );

            // 更新发件组状态为发送中
            await sqlContext.SendingGroups.UpdateAsync(
                x => x.Id == group.Id,
                x => x.SetProperty(y => y.Status, SendingGroupStatus.Sending)
            );

            return result;
        }

        /// <summary>
        /// 发送模块调用该方法获取发件项
        /// 若返回空，会导致发送任务暂停
        /// </summary>
        /// <returns></returns>
        public async Task<SendItemMeta?> GetEmailItem(SendingContext sendingContext)
        {
            var outbox = sendingContext.OutboxAddress;
            if (outbox == null)
            {
                _logger.Error("GetSendItem 调用失败, 请先获取发件箱");
                return null;
            }

            // 获取用户的发件任务
            // 发件任务可能每次都为空，导致无法获取到有效的发件项，需要避免
            var userGroupTasksPool = GetUserGroupTasksPool(outbox.UserId);
            if (userGroupTasksPool == null)
            {
                return null;
            }

            return await userGroupTasksPool.GetEmailItem(sendingContext);
        }

        /// <summary>
        /// 获取用户的发件任务池
        /// 已经对空的发件池进行了处理
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>
        /// 为 null 则表示无发件任务 或者 发件任务池已被释放
        /// </returns>
        private UserGroupTasksPool? GetUserGroupTasksPool(long userId)
        {
            if (userTasksPools.IsEmpty)
            {
                _logger.Info($"获取发件池失败，用户 {userId} 发件任务池为空");
                return null;
            }

            // 依次获取发件项
            if (!userTasksPools.TryGetValue(userId, out var sendingGroupsPool))
            {
                _logger.Info($"无法获取用户 {userId} 发件任务队列，该队列已释放");
                return null;
            }

            // 为空时移除
            if (sendingGroupsPool.IsEmpty)
            {
                // 移除自己
                userTasksPools.TryRemove(userId, out _);
                return null;
            }

            return sendingGroupsPool;
        }

        /// <summary>
        /// 移除发件任务
        /// 单纯移除，不更新状态与触发通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public void RemoveSendingGroupTask(long userId, long sendingGroupId)
        {
            if (!userTasksPools.TryGetValue(userId, out var userSendingGroupsPool))
                return;
            userSendingGroupsPool.TryRemove(sendingGroupId, out _);
        }
    }
}
