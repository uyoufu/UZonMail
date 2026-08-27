using log4net;
using MailKit.Security;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Extensions;

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts
{
    /// <summary>
    /// 发件箱地址
    /// 该地址可能仅用于部分发件箱
    /// 也有可能是用于通用发件
    /// </summary>
    public class SenderEmailAddress : EmailAddress
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SenderEmailAddress));

        #region 私有变量
        // 发件箱数据
        public SenderAccount SenderAccount { get; private set; }
        public SenderCredentialSnapshot Credentials { get; }

        /// <summary>
        /// 发送目录的 id
        /// </summary>
        private readonly object _stateLock = new();
        private readonly HashSet<SendingTargetId> _sendingTargetIds = [];
        private long _cooldownUntilUtcTicks;

        /// <summary>
        /// 开始日期
        /// </summary>
        private DateOnly _sentCountDateUtc;
        private long _quotaBlockedUntilUtcTicks;
        #endregion

        #region 公开属性
        public SendingProtocol SendingProtocol => SenderAccount.Protocol;

        public SenderEmailAddressType Type { get; private set; } = SenderEmailAddressType.Specific;

        /// <summary>
        /// 当前发件箱结束冷却并可再次参与调度的 UTC 时间。
        /// </summary>
        public DateTimeOffset CooldownUntilUtc =>
            new(Interlocked.Read(ref _cooldownUntilUtcTicks), TimeSpan.Zero);

        /// <summary>
        /// 每日额度重置前不可参与调度的 UTC 时间。
        /// </summary>
        public DateTimeOffset QuotaBlockedUntilUtc =>
            new(Interlocked.Read(ref _quotaBlockedUntilUtcTicks), TimeSpan.Zero);

        /// <summary>
        /// 当前发送计数所属的 UTC 日期。
        /// </summary>
        public DateOnly SentCountDateUtc
        {
            get
            {
                lock (_stateLock)
                    return _sentCountDateUtc;
            }
        }

        /// <summary>
        /// 用户 ID
        /// </summary>
        public long UserId => SenderAccount.UserId;

        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; private set; }

        /// <summary>
        /// 授权用户名
        /// </summary>
        public string? SmtpAuthUserName
        {
            get { return Credentials.Smtp?.LoginName ?? SenderAccount.Email; }
        }

        /// <summary>
        /// Outlook 的授权用户名
        /// 实际当成 clientId 在使用
        /// </summary>
        public string OutlookClientId => Credentials.OAuth?.ClientId ?? string.Empty;

        /// <summary>
        /// 授权密码或者 OAuth 的 secrete
        /// </summary>
        public string? PlainPassword => Credentials.Smtp?.Password;

        /// <summary>
        /// SMTP 服务器地址
        /// </summary>
        public string SmtpHost => Credentials.Smtp?.Host ?? string.Empty;

        /// <summary>
        /// SMTP 端口
        /// </summary>
        public int SmtpPort => Credentials.Smtp?.Port ?? 0;

        /// <summary>
        /// 开启 SSL
        /// </summary>
        //public bool EnableSSL => SenderAccount.EnableSSL;
        public ConnectionSecurity ConnectionSecurity =>
            Credentials.Smtp?.ConnectionSecurity ?? ConnectionSecurity.None;

        /// <summary>
        /// 单日最大发送数量
        /// 为 0 时表示不限制
        /// </summary>
        public int MaxSendCountPerDay => SenderAccount.MaxSendCountPerDay;

        /// <summary>
        /// 当天合计发件
        /// 成功失败都被计算在内
        /// </summary>
        public int SentTotalToday { get; private set; }

        /// <summary>
        /// 本次合计发件
        /// </summary>
        public int SentTotal { get; private set; }

        /// <summary>
        /// 递增发送数量
        /// 或跨越天数, 重置发送数量
        /// 对于发件箱来说，是单线程，因此不需要考虑并发问题
        /// </summary>
        public void IncreaseSentCount()
        {
            lock (_stateLock)
            {
                SentTotal++;
                var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
                if (_sentCountDateUtc != utcToday)
                {
                    _sentCountDateUtc = utcToday;
                    SentTotalToday = 1;
                }
                else
                {
                    SentTotalToday++;
                }
            }
        }

        /// <summary>
        /// 代理 Id
        /// </summary>
        public long ProxyId => SenderAccount.ProxyId ?? 0;

        /// <summary>
        /// 回复至邮箱
        /// </summary>
        public List<string> ReplyToEmails { get; set; } = [];

        /// <summary>
        /// 错误原因
        /// </summary>
        public string ErroredMessage { get; private set; } = "";

        /// <summary>
        /// 是否应释放
        /// 只有发件箱无法再次被使用时，才会被标记为应释放
        /// </summary>
        public bool ShouldDispose { get; private set; } = false;

        public bool IsPermanentlyInvalid { get; private set; }

        /// <summary>
        /// 工作中
        /// 当没有发送目标后，working 为 false
        /// </summary>
        public bool IsWorking
        {
            get
            {
                lock (_stateLock)
                    return _sendingTargetIds.Count > 0;
            }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Enable
        {
            get => !ShouldDispose && IsWorking;
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 生成发件地址
        /// </summary>
        /// <param name="senderAccount"></param>
        /// <param name="sendingGroupId"></param>
        /// <param name="type"></param>
        /// <param name="sendingItemIds"></param>
        public SenderEmailAddress(
            SenderAccount senderAccount,
            SenderCredentialSnapshot credentials,
            long sendingGroupId,
            SenderEmailAddressType type,
            List<long>? sendingItemIds = null
        )
        {
            SenderAccount = senderAccount;
            Credentials = credentials;
            Type = type;

            // 共享发件箱
            if (Type.HasFlag(SenderEmailAddressType.Shared))
                _sendingTargetIds.Add(new SendingTargetId(sendingGroupId));

            // 特定发件箱
            if (sendingItemIds != null)
            {
                if (!Type.HasFlag(SenderEmailAddressType.Specific))
                    throw new Exception("特定发件箱的 Type 必须包含 Specific");

                // 开始添加
                sendingItemIds?.ForEach(x =>
                    _sendingTargetIds.Add(new SendingTargetId(sendingGroupId, x))
                );
            }

            CreateDate = DateTime.UtcNow;
            Email = senderAccount.Email;
            Name = senderAccount.Name;
            Id = senderAccount.Id;

            ReplyToEmails = senderAccount.ReplyToEmails.SplitBySeparators().Distinct().ToList();
            SentTotalToday = senderAccount.SentTotalToday;
            // 旧数据没有计数日期，首次加载时按当天计数，避免升级后意外突破日限额。
            _sentCountDateUtc =
                senderAccount.SentCountDateUtc ?? DateOnly.FromDateTime(DateTime.UtcNow);
            if (
                senderAccount.MaxSendCountPerDay > 0
                && _sentCountDateUtc == DateOnly.FromDateTime(DateTime.UtcNow)
                && SentTotalToday >= senderAccount.MaxSendCountPerDay
            )
                ScheduleDailyQuotaReset(DateTimeOffset.UtcNow);
            Weight = senderAccount.Weight > 0 ? senderAccount.Weight : 1;
        }
        #endregion

        #region 更新发件箱
        /// <summary>
        /// 使用 SenderEmailAddress 更新既有的发件地址
        /// 非并发操作
        /// </summary>
        /// <param name="data"></param>
        public void Update(SenderEmailAddress data)
        {
            lock (_stateLock)
            {
                Type |= data.Type;
                Weight = data.Weight;
                ReplyToEmails = data.ReplyToEmails;

                foreach (var targetId in data.GetSendingTargetsSnapshot())
                    _sendingTargetIds.Add(targetId);
            }
        }
        #endregion

        #region 外部调用，改变内部状态

        /// <summary>
        /// 是否被禁用
        /// </summary>
        /// <returns></returns>
        public bool IsLimited()
        {
            return MaxSendCountPerDay > 0 && SentTotalToday >= MaxSendCountPerDay;
        }

        /// <summary>
        /// 是否包含指定的发件组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool ContainsSendingGroup(long sendingGroupId)
        {
            lock (_stateLock)
                return _sendingTargetIds.Any(x => x.SendingGroupId == sendingGroupId);
        }

        /// <summary>
        /// 获取发件箱在指定发送组内的绑定类型，避免其它组的共享绑定污染当前组。
        /// </summary>
        public SenderEmailAddressType GetTypeForSendingGroup(long sendingGroupId)
        {
            lock (_stateLock)
            {
                var groupTargets = _sendingTargetIds.Where(x => x.SendingGroupId == sendingGroupId);
                var type = SenderEmailAddressType.None;
                foreach (var target in groupTargets)
                {
                    type |=
                        target.SendingItemId > 0
                            ? SenderEmailAddressType.Specific
                            : SenderEmailAddressType.Shared;
                }
                return type;
            }
        }

        /// <summary>
        /// 获取发件组 id
        /// </summary>
        /// <returns></returns>
        public List<long> GetSendingGroupIds()
        {
            lock (_stateLock)
                return [.. _sendingTargetIds.Select(x => x.SendingGroupId).Distinct()];
        }

        /// <summary>
        /// 获取指定了发件箱的邮件
        /// </summary>
        /// <returns></returns>
        public List<long> GetSpecificSendingItemIds()
        {
            lock (_stateLock)
                return
                [
                    .. _sendingTargetIds
                        .Where(x => x.SendingItemId > 0)
                        .Select(x => x.SendingItemId),
                ];
        }

        /// <summary>
        /// 获取指定发送组中绑定当前发件箱的邮件 ID。
        /// </summary>
        public List<long> GetSpecificSendingItemIds(long sendingGroupId)
        {
            lock (_stateLock)
                return
                [
                    .. _sendingTargetIds
                        .Where(x => x.SendingGroupId == sendingGroupId && x.SendingItemId > 0)
                        .Select(x => x.SendingItemId),
                ];
        }

        /// <summary>
        /// 移除指定的发件项
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <param name="sendingItemId"></param>
        public void RemoveSepecificSendingItem(long sendingGroupId, long sendingItemId)
        {
            lock (_stateLock)
                _sendingTargetIds.Remove(new SendingTargetId(sendingGroupId, sendingItemId));
        }

        /// <summary>
        /// 移除指定发送组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        public void RemoveSendingGroup(long sendingGroupId)
        {
            lock (_stateLock)
                _sendingTargetIds.RemoveWhere(x => x.SendingGroupId == sendingGroupId);
        }

        /// <summary>
        /// 将发件箱置为冷却状态。冷却只记录资格时间，不占用发送工作槽。
        /// </summary>
        public void ScheduleCooldown(TimeSpan cooldown, DateTimeOffset utcNow)
        {
            if (cooldown <= TimeSpan.Zero)
                return;

            var newUntilTicks = utcNow.Add(cooldown).UtcTicks;
            while (true)
            {
                var currentTicks = Interlocked.Read(ref _cooldownUntilUtcTicks);
                if (currentTicks >= newUntilTicks)
                    return;
                if (
                    Interlocked.CompareExchange(
                        ref _cooldownUntilUtcTicks,
                        newUntilTicks,
                        currentTicks
                    ) == currentTicks
                )
                    return;
            }
        }

        /// <summary>
        /// 判断发件箱当前是否具备调度资格。
        /// </summary>
        public bool IsEligible(DateTimeOffset utcNow) =>
            !ShouldDispose
            && IsWorking
            && CooldownUntilUtc <= utcNow
            && QuotaBlockedUntilUtc <= utcNow;

        /// <summary>
        /// 获取冷却或额度限制结束后的最早调度时间。
        /// </summary>
        public DateTimeOffset NextEligibleUtc =>
            CooldownUntilUtc >= QuotaBlockedUntilUtc ? CooldownUntilUtc : QuotaBlockedUntilUtc;

        /// <summary>
        /// 将发件箱阻塞到下一个 UTC 自然日，并保留其组绑定供自动恢复。
        /// </summary>
        public void ScheduleDailyQuotaReset(DateTimeOffset utcNow)
        {
            var nextUtcDay = new DateTimeOffset(utcNow.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);
            Interlocked.Exchange(ref _quotaBlockedUntilUtcTicks, nextUtcDay.UtcTicks);
        }

        /// <summary>
        /// 判断发件箱是否正在等待每日额度重置。
        /// </summary>
        public bool IsQuotaBlocked(DateTimeOffset utcNow) => QuotaBlockedUntilUtc > utcNow;

        private IReadOnlyList<SendingTargetId> GetSendingTargetsSnapshot()
        {
            lock (_stateLock)
                return [.. _sendingTargetIds];
        }

        /// <summary>
        /// 标记应该释放
        /// </summary>
        /// <param name="erroredMessage"></param>
        public void MarkShouldDispose(string erroredMessage)
        {
            ErroredMessage = erroredMessage;
            ShouldDispose = true;
        }

        public void MarkInvalid(string errorMessage)
        {
            IsPermanentlyInvalid = true;
            MarkShouldDispose(errorMessage);
        }

        private int _isRunningInTask = 0;

        public bool TryMarkTaskRunning()
        {
            return Interlocked.CompareExchange(ref _isRunningInTask, 1, 0) == 0;
        }

        public void MarkTaskStopped()
        {
            Interlocked.Exchange(ref _isRunningInTask, 0);
        }

        /// <summary>
        /// 是否正在任务中运行
        /// </summary>
        public bool IsRunningInTask => Volatile.Read(ref _isRunningInTask) > 0;
        #endregion
    }
}
