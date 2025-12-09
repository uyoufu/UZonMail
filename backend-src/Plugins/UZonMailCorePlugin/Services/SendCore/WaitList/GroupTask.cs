using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Database.SQL.EmailSending;
using UZonMail.CorePlugin.Services.Encrypt;
using UZonMail.CorePlugin.Services.Encrypt.Models;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.EmailWaitList;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.Proxies;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.CorePlugin.SignalRHubs.Extensions;
using UZonMail.CorePlugin.SignalRHubs.SendEmail;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.WaitList
{
    /// <summary>
    /// 一次发件任务
    /// 不同的发件任务包含不同的数据库上下文，保证上下文不重复使用
    /// 包含发件组和收件内容
    /// 请调用 <seealso cref="Create(SendingContext, long)"/> 方法创建实例
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class GroupTask(EncryptService encryptService) : ITransientService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GroupTask));
        private readonly object _lockObject = new();

        /// <summary>
        /// 创建一个发件任务
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public static async Task<GroupTask?> Create(SendingContext ctx, long sendingGroupId)
        {
            var groupTask = ctx.Provider.GetRequiredService<GroupTask>();
            // 初始化基本参数
            groupTask.SetSendingGroupId(sendingGroupId);
            // 初始化组
            if (!await groupTask.InitSendingGroup(ctx))
                return null;
            return groupTask;
        }

        #region 属性
        /// <summary>
        /// 组 id
        /// </summary>
        public long SendingGroupId { get; private set; }

        /// <summary>
        /// 通过组 id 获取的组
        /// </summary>
        private SendingGroup _sendingGroup;

        /// <summary>
        /// 密钥
        /// </summary>
        public EncryptParams SmtpPasswordSecretKeys => encryptService.GetEncrypParams();

        /// <summary>
        /// 所属用户
        /// </summary>
        public long UserId { get; private set; }

        /// <summary>
        /// 发件项数据
        /// 此处只保存自由发件的项
        /// 若指定发件箱，数据 id 会保存在 outbox 中
        /// </summary>
        private readonly SendingItemMetaList _sendingItemMetas = new();

        /// <summary>
        /// 可用的代理
        /// </summary>
        private List<long> ProxyIds { get; set; } = [];

        /// <summary>
        /// 可用的模板
        /// </summary>
        private UsableTemplateList _usableTemplates;

        /// <summary>
        /// 是否应该释放
        /// </summary>
        public bool ShouldDispose => _sendingItemMetas.ToSendingCount == 0;

        /// <summary>
        /// 任务开始日期
        /// </summary>
        private readonly DateTime _startDate = DateTime.UtcNow;
        #endregion

        #region 初始化
        public void SetSendingGroupId(long sendingGroupId)
        {
            SendingGroupId = sendingGroupId;
        }
        #endregion

        /// <summary>
        /// 初始化发件组信息
        /// 只能被调用一次
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private async Task<bool> InitSendingGroup(SendingContext sendingContext)
        {
            var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();

            // 获取完整的邮件组
            var sendingGroup = await sqlContext
                .SendingGroups.AsNoTracking()
                .Where(x => x.Id == SendingGroupId)
                .Include(x => x.Outboxes)
                .Include(x => x.Templates)
                .FirstOrDefaultAsync();

            if (sendingGroup == null)
            {
                _logger.Error($"发件组 {SendingGroupId} 不存在");
                return false;
            }
            // 保存发件组
            _sendingGroup = sendingGroup;

            // 将收件箱重置为空，方便垃圾回收
            sendingGroup.Inboxes = [];

            // 正在发送时，不添加
            if (sendingGroup.Status == SendingGroupStatus.Sending)
            {
                _logger.Error($"发件组 {SendingGroupId} 正在发送中");
                return false;
            }

            // 更新用户 id
            UserId = sendingGroup.UserId;

            // 将公共的发件箱添加到发件池中
            // 邮件级别的发件箱在初始化发送项时，再添加
            await AddSharedOutboxToPool(
                sendingContext,
                sendingGroup.Outboxes,
                sendingGroup.OutboxGroups
            );

            // 保存所使用的代理
            ProxyIds = sendingGroup.ProxyIds ?? [];

            // 更新代理缓存
            var proxyManager = sendingContext.Provider.GetRequiredService<ProxiesManager>();
            await proxyManager.UpdateUserProxies(sendingContext.Provider, UserId);

            // 获取所有的模板，模板是用户级别的
            _usableTemplates = new UsableTemplateList(UserId);
            // 添加组的通用模板
            _usableTemplates.AddSendingGroupTemplates(
                _sendingGroup.Templates!.ConvertAll(x => x.Id)
            );

            return true;
        }

        /// <summary>
        /// 将新的发件箱添加到发件池中
        /// 会自动去重
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="outboxes"></param>
        /// <param name="outboxGroup"></param>
        /// <returns></returns>
        private async Task AddSharedOutboxToPool(
            SendingContext sendingContext,
            List<Outbox> outboxes,
            List<IdAndName>? outboxGroup
        )
        {
            if (outboxes.Count == 0 && (outboxGroup == null || outboxGroup.Count == 0))
                return;
            var container = sendingContext.Provider.GetRequiredService<OutboxesManager>();

            var outboxAddresses = outboxes.ConvertAll(x => new OutboxEmailAddress(
                x,
                SendingGroupId,
                SmtpPasswordSecretKeys,
                OutboxEmailAddressType.Shared
            ));
            foreach (var outbox in outboxAddresses)
            {
                container.AddOutbox(outbox);
            }

            // 解析发件箱组
            if (outboxGroup == null || outboxGroup.Count == 0)
                return;
            var outboxGroupIds = outboxGroup.Select(x => x.Id).ToList();
            // 添加发件组的发件箱
            var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();
            var groupBoxes = await sqlContext
                .Outboxes.AsNoTracking()
                .Where(x => outboxGroupIds.Contains(x.EmailGroupId))
                .ToListAsync();
            await AddSharedOutboxToPool(sendingContext, groupBoxes, null);
        }

        /// <summary>
        /// 初始化发件组的发件项
        /// 允许被被多次调用，该接口会查找 SendingItemStatus.CanSend 的项
        /// 多次调用的场景: 重发部分发件项
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="sendingItemIds">只发送特定的发件项</param>
        /// <returns></returns>
        public async Task<bool> InitSendingItems(
            SendingContext sendingContext,
            List<long>? sendingItemIds
        )
        {
            var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();
            // 获取待发件
            var dbSet = sqlContext
                .SendingItems.AsNoTracking()
                .Where(x => x.SendingGroupId == SendingGroupId)
                .Where(x =>
                    x.Status == SendingItemStatus.Created || x.Status == SendingItemStatus.Failed
                ); // 获取可发送项

            if (sendingItemIds != null && sendingItemIds.Count > 0)
            {
                dbSet = dbSet.Where(x => sendingItemIds.Contains(x.Id));
            }

            // 只获取需要的数据，完整数据在执行过程中再次获取
            List<SendingItem> toSendingItems = await dbSet
                .Select(x => new SendingItem()
                {
                    EmailTemplateId = x.EmailTemplateId,
                    Id = x.Id,
                    OutBoxId = x.OutBoxId,
                    Inboxes = x.Inboxes,
                    ProxyId = x.ProxyId
                })
                .ToListAsync();

            // 去掉当前组中已经包含的项
            var existIds = _sendingItemMetas.SendingItemIds.ToList();
            toSendingItems = toSendingItems.Where(x => !existIds.Contains(x.Id)).ToList();

            // 获取发件箱
            HashSet<long> outboxIds = [.. toSendingItems.Select(x => x.OutBoxId).Where(x => x > 0)];
            var outboxes = await sqlContext
                .Outboxes.Where(x => outboxIds.Contains(x.Id))
                .ToListAsync();

            // 对于找不到发件项的邮件，给予标记非法
            var invalidSendingItemIds = toSendingItems
                .Where(x => x.OutBoxId > 0)
                .Where(x =>
                {
                    var outbox = outboxes.FirstOrDefault(o => o.Id == x.OutBoxId);
                    return outbox == null;
                })
                .Select(x => x.Id)
                .ToList();
            if (invalidSendingItemIds.Count > 0)
            {
                // 更新邮件状态
                await sqlContext.SendingItems.UpdateAsync(
                    x => invalidSendingItemIds.Contains(x.Id),
                    x =>
                        x.SetProperty(y => y.Status, SendingItemStatus.Invalid)
                            .SetProperty(y => y.SendDate, DateTime.UtcNow)
                            .SetProperty(y => y.SendResult, "指定的发件箱已被删除")
                );

                // 过滤掉无效的发件箱项
                toSendingItems =
                [
                    .. toSendingItems.FindAll(x => !invalidSendingItemIds.Contains(x.Id))
                ];
            }

            // 按注册的发件项过滤无效项
            var sendingItemFilters = sendingContext.Provider.GetServices<ISendingItemFilter>();
            var filteredInvalidIds = new List<long>();
            foreach (var filter in sendingItemFilters)
            {
                var filterInvalidIds = await filter.GetInvalidSendingItemIds(toSendingItems);
                filteredInvalidIds.AddRange(filterInvalidIds);
            }
            var filteredInvalidIdsSet = filteredInvalidIds.ToHashSet();
            if (filteredInvalidIdsSet.Count > 0)
            {
                toSendingItems = toSendingItems.FindAll(x => !filteredInvalidIdsSet.Contains(x.Id));
            }

            // 更新待发件列表
            // 由于初始化时，不是并发的，不需要加锁
            var toSendingItemMetas = toSendingItems
                .Select(x => new SendItemMeta(x.Id, x.OutBoxId))
                .ToList();
            _sendingItemMetas.AddRange(toSendingItemMetas);

            // 添加代理和模板
            foreach (var toSendingItem in toSendingItems)
            {
                // 添加特定模板
                _usableTemplates.AddSendingItemTemplate(
                    toSendingItem.Id,
                    toSendingItem.EmailTemplateId
                );

                // 添加特定代理
                //_usableProxies.AddSendingItemProxy(toSendingItem.Id, toSendingItem.ProxyId);
            }

            // 更新当前发件组的数据
            await UpdateSendingGroupInfo(sqlContext, SendingGroupId);

            // 新增特定发件箱
            var outboxesPoolList = sendingContext.Provider.GetRequiredService<OutboxesManager>();
            foreach (var outbox in outboxes)
            {
                // 获取收件项 Id
                var sendingItemIdsTemp = toSendingItems
                    .Where(x => x.OutBoxId == outbox.Id)
                    .Select(x => x.Id)
                    .ToList();
                var outboxAddress = new OutboxEmailAddress(
                    outbox,
                    SendingGroupId,
                    SmtpPasswordSecretKeys,
                    OutboxEmailAddressType.Specific,
                    sendingItemIdsTemp
                );
                outboxesPoolList.AddOutbox(outboxAddress);
            }

            // 将发件项修改为发送中
            if (toSendingItems.Count > 0)
            {
                await sqlContext.SendingItems.UpdateAsync(
                    x => toSendingItems.Select(x => x.Id).Contains(x.Id),
                    x => x.SetProperty(y => y.Status, SendingItemStatus.Pending)
                );
            }

            // 通知用户，任务已开始
            var client = sendingContext.HubClient.GetUserClient(UserId);
            await client.SendingGroupProgressChanged(
                new SendingGroupProgressArg(_sendingGroup, _startDate)
                {
                    ProgressType = ProgressType.Start
                }
            );

            return true;
        }

        /// <summary>
        /// 更新发件组信息，并保存到数据库中
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        private async Task UpdateSendingGroupInfo(SqlContext sqlContext, long sendingGroupId)
        {
            // 获取成功数、失败数、总数
            var sendingGroup = await sqlContext
                .SendingGroups.Where(x => x.Id == sendingGroupId)
                .FirstOrDefaultAsync();
            if (sendingGroup == null)
                return;

            var allSendingItems = await sqlContext
                .SendingItems.AsNoTracking()
                .Where(x => x.SendingGroupId == sendingGroup.Id)
                .Select(x => new { x.Status, x.SendDate, })
                .ToListAsync();

            // 更新发件组的信息
            sendingGroup.Status = SendingGroupStatus.Sending;
            sendingGroup.TotalCount = allSendingItems.Count;
            sendingGroup.SuccessCount = allSendingItems.Count(x =>
                x.Status >= SendingItemStatus.Success
            );
            // 有发送日期的项，表示已经发送过了
            sendingGroup.SentCount =
                allSendingItems.Count(x => x.Status <= SendingItemStatus.Cancel)
                + sendingGroup.SuccessCount;
            // 开始发送日期
            if (sendingGroup.SendStartDate == DateTime.MinValue)
                sendingGroup.SendStartDate = DateTime.UtcNow;
            // 保存组状态
            await sqlContext.SaveChangesAsync();

            // 更新到当前类中
            _sendingGroup.Status = sendingGroup.Status;
            _sendingGroup.TotalCount = sendingGroup.TotalCount;
            _sendingGroup.SuccessCount = sendingGroup.SuccessCount;
            _sendingGroup.SentCount = sendingGroup.SentCount;
            _sendingGroup.SendStartDate = sendingGroup.SendStartDate;
        }

        /// <summary>
        /// 获取发件项
        /// </summary>
        /// <returns></returns>
        public async Task<SendItemMeta?> GetEmailItem(SendingContext sendingContext)
        {
            // 保存当前组的开始日期
            sendingContext.GroupTaskStartDate = _startDate;

            var outbox = sendingContext.OutboxAddress;
            if (outbox == null)
                return null;

            // 判断是否为当前组对应的发件箱
            if (!outbox.ContainsSendingGroup(SendingGroupId))
                return null;

            // 从列表中移除发件项并转换成 sendItem
            var sendItemMeta = await GetEmailItemFromDb(sendingContext);
            if (sendItemMeta == null)
                return null;

            // 为 sendItem 动态赋值
            // 赋予发件箱
            sendItemMeta.SetOutbox(outbox);

            var sendingSetting = await sendingContext
                .Provider.GetRequiredService<AppSettingsManager>()
                .GetSetting<SendingSetting>(
                    sendingContext.SqlContext,
                    sendItemMeta.SendingItem.UserId
                );
            sendItemMeta.SetReplyToEmails(outbox.ReplyToEmails, sendingSetting.ReplyToEmailsList);

            // 推送开始发件
            var client = sendingContext.HubClient.GetUserClient(UserId);
            client?.SendingItemStatusChanged(
                new SendingItemStatusChangedArg(sendItemMeta.SendingItem)
                {
                    Status = SendingItemStatus.Sending
                }
            );

            return sendItemMeta;
        }

        /// <summary>
        /// 从数据库中获取发件项
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private async Task<SendItemMeta?> GetEmailItemFromDb(SendingContext sendingContext)
        {
            var outbox =
                sendingContext.OutboxAddress ?? throw new Exception("GetSendItem 调用失败, 请先获取发件箱");

            // 先发指定项
            SendItemMeta? sendItemMeta = null;
            if (outbox.Type.HasFlag(OutboxEmailAddressType.Specific))
            {
                // 获取特定项
                sendItemMeta = _sendingItemMetas.GetSendingMeta(outbox.Id);
            }

            // 若特定项已经发完，则从共享项中获取
            if (sendItemMeta == null && outbox.Type.HasFlag(OutboxEmailAddressType.Shared))
            {
                sendItemMeta = _sendingItemMetas.GetSendingMeta();
            }
            if (sendItemMeta == null)
                return null;

            // 如果已经包含 SendingItem, 说明初始化过了，直接返回
            if (sendItemMeta.Initialized)
            {
                return sendItemMeta;
            }
            sendItemMeta.Initialized = true;

            // 添加局部使用的代理
            var sqlContext = sendingContext.SqlContext;
            sendItemMeta.AvailableProxyIds = ProxyIds;

            // 拉取发件项
            sendItemMeta = await _sendingItemMetas.FillSendingItem(sqlContext, sendItemMeta);

            // 获取附件
            await sendItemMeta.ResolveAttachments(sendingContext);

            // 生成正文原始内容
            var originBody = await GetSendingItemOriginBody(
                sqlContext,
                sendItemMeta.SendingItem,
                sendItemMeta.BodyData
            );
            await sendItemMeta.SetHtmlBody(sendingContext, originBody);

            // 设置主题
            var subject = GetSubject(sendItemMeta.BodyData);
            await sendItemMeta.SetSubject(sendingContext, subject);

            // 添加最大重试次数
            var sendingSetting = await sendingContext
                .Provider.GetRequiredService<AppSettingsManager>()
                .GetSetting<SendingSetting>(sqlContext, sendItemMeta.SendingItem.UserId);
            await sendItemMeta.SetMaxRetryCount(sendingSetting.MaxRetryCount);

            return sendItemMeta;
        }

        /// <summary>
        /// 获取原始发件内容
        /// 变量未经过处理
        /// </summary>
        /// <param name="sendItemMeta"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        private async Task<string> GetSendingItemOriginBody(
            SqlContext sqlContext,
            SendingItem sendingItem,
            SendingItemExcelData? bodyData
        )
        {
            // 批量发送的情况
            if (sendingItem.IsSendingBatch)
            {
                if (!string.IsNullOrEmpty(_sendingGroup.Body))
                    return _sendingGroup.Body;

                var template = await _usableTemplates.GetTemplate(sqlContext, sendingItem.Id);
                return template?.Content ?? string.Empty;
            }

            // 非批量发送
            // 当有用户数据时
            if (bodyData != null)
            {
                // 判断是否有 body
                if (!string.IsNullOrEmpty(bodyData.Body))
                    return bodyData.Body;

                // 判断是否有模板 id 或者模板名称
                if (bodyData.TemplateId > 0)
                {
                    var template = await _usableTemplates.GetTemplateById(
                        sqlContext,
                        bodyData.TemplateId
                    );
                    if (template != null)
                        return template.Content;
                }

                // 判断是否有模板名称
                if (!string.IsNullOrEmpty(bodyData.TemplateName))
                {
                    var template = await _usableTemplates.GetTemplateByName(
                        sqlContext,
                        bodyData.TemplateName
                    );
                    if (template != null)
                        return template.Content;
                }
            }

            // 没有数据时，优先使用组中的 body
            if (!string.IsNullOrEmpty(_sendingGroup.Body))
                return _sendingGroup.Body;

            // 返回随机模板
            var randTemplate = await _usableTemplates.GetTemplate(sqlContext, sendingItem.Id);
            return randTemplate?.Content ?? string.Empty;
        }

        /// <summary>
        /// 获取主题
        /// 随机主题只需要在发送时确定即可
        /// </summary>
        /// <returns></returns>
        private string GetSubject(SendingItemExcelData? bodyData)
        {
            // 若本身有主题，则使用自身的主题
            if (bodyData != null && !string.IsNullOrEmpty(bodyData.Subject))
            {
                return bodyData.Subject;
            }
            return _sendingGroup.GetRandSubject();
        }

        /// <summary>
        /// 判断 outbox 能否匹配到发件项
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public bool MatchEmailItem(OutboxEmailAddress outbox)
        {
            // 特定发件箱
            if (outbox.Type.HasFlag(OutboxEmailAddressType.Specific))
            {
                var matchSpecific = _sendingItemMetas.MatchSendingMeta(outbox.Id, true);
                if (matchSpecific)
                    return true;
            }

            // 从当前组中获取
            if (outbox.Type.HasFlag(OutboxEmailAddressType.Shared))
            {
                var matchShared = _sendingItemMetas.MatchSendingMeta(outbox.Id, false);
                if (matchShared)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 移除指定发件箱的发件项
        /// </summary>
        /// <param name="outboxEmail"></param>
        /// <returns></returns>
        public void RemovePendingItems(List<long> sendingItemIds)
        {
            sendingItemIds.ForEach(x => _sendingItemMetas.RemovePendingItem(x));
        }
    }
}
