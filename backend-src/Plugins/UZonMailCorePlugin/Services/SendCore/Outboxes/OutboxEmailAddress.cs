using log4net;
using UZonMail.CorePlugin.Services.Encrypt.Models;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Extensions;

namespace UZonMail.CorePlugin.Services.SendCore.Outboxes
{
    /// <summary>
    /// 发件箱地址
    /// 该地址可能仅用于部分发件箱
    /// 也有可能是用于通用发件
    /// </summary>
    public class OutboxEmailAddress : EmailAddress
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OutboxEmailAddress));

        #region 私有变量
        // 发件箱数据
        public Outbox Outbox { get; private set; }

        /// <summary>
        /// 发送目录的 id
        /// </summary>
        private HashSet<SendingTargetId> _sendingTargetIds = [];

        /// <summary>
        /// 开始日期
        /// </summary>
        private DateTime _startDate = DateTime.UtcNow;
        #endregion

        #region 公开属性
        public OutboxEmailAddressType Type { get; private set; } = OutboxEmailAddressType.Specific;

        /// <summary>
        /// 用户 ID
        /// </summary>
        public long UserId => Outbox.UserId;

        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; private set; }

        /// <summary>
        /// 授权用户名
        /// </summary>
        public string? SmtpAuthUserName
        {
            get { return string.IsNullOrEmpty(Outbox.UserName) ? Outbox.Email : Outbox.UserName; }
        }

        /// <summary>
        /// Outlook 的授权用户名
        /// 实际当成 clientId 在使用
        /// </summary>
        public string OutlookClientId => Outbox.UserName ?? string.Empty;

        //public OutboxAuthType AuthType => _outbox.AuthType;

        //public string ClientId => _outbox.ClientId ?? string.Empty;

        //public string TenantId => _outbox.TenantId ?? string.Empty;

        /// <summary>
        /// 授权密码或者 OAuth 的 secrete
        /// </summary>
        public string? AuthPassword { get; private set; }

        /// <summary>
        /// SMTP 服务器地址
        /// </summary>
        public string SmtpHost => Outbox.SmtpHost;

        /// <summary>
        /// SMTP 端口
        /// </summary>
        public int SmtpPort => Outbox.SmtpPort;

        /// <summary>
        /// 开启 SSL
        /// </summary>
        public bool EnableSSL => Outbox.EnableSSL;

        /// <summary>
        /// 单日最大发送数量
        /// 为 0 时表示不限制
        /// </summary>
        public int MaxSendCountPerDay => Outbox.MaxSendCountPerDay;

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
            SentTotal++;

            // 重置每日发件量
            if (_startDate.Date != DateTime.UtcNow.Date)
            {
                _startDate = DateTime.UtcNow;
                SentTotalToday = 0;
            }
            else
            {
                SentTotalToday++;
            }
        }

        /// <summary>
        /// 代理 Id
        /// </summary>
        public long ProxyId => Outbox.ProxyId;

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

        /// <summary>
        /// 工作中
        /// 当没有发送目标后，working 为 false
        /// </summary>
        public bool IsWorking => _sendingTargetIds.Count > 0;

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
        /// <param name="outbox"></param>
        /// <param name="sendingGroupId"></param>
        /// <param name="type"></param>
        /// <param name="sendingItemIds"></param>
        public OutboxEmailAddress(
            Outbox outbox,
            long sendingGroupId,
            EncryptParams encrypParams,
            OutboxEmailAddressType type,
            List<long> sendingItemIds = null
        )
        {
            Outbox = outbox;
            AuthPassword = outbox.Password.DeAES(encrypParams.Key, encrypParams.Iv);
            Type = type;

            // 共享发件箱
            if (Type.HasFlag(OutboxEmailAddressType.Shared))
            {
                _sendingTargetIds.Add(new SendingTargetId(sendingGroupId));
            }

            if (sendingItemIds != null)
            {
                if (!Type.HasFlag(OutboxEmailAddressType.Specific))
                    throw new Exception("特定发件箱的 Type 必须包含 Specific");

                // 开始添加
                sendingItemIds?.ForEach(x =>
                    _sendingTargetIds.Add(new SendingTargetId(sendingGroupId, x))
                );
            }

            CreateDate = DateTime.UtcNow;
            Email = outbox.Email;
            Name = outbox.Name;
            Id = outbox.Id;

            ReplyToEmails = outbox.ReplyToEmails.SplitBySeparators().Distinct().ToList();
            SentTotalToday = outbox.SentTotalToday;
            Weight = outbox.Weight > 0 ? outbox.Weight : 1;
        }
        #endregion

        #region 更新发件箱
        /// <summary>
        /// 使用 OutboxEmailAddress 更新既有的发件地址
        /// 非并发操作
        /// </summary>
        /// <param name="data"></param>
        public void Update(OutboxEmailAddress data)
        {
            // 更新类型
            Type |= data.Type;
            Weight = data.Weight;
            ReplyToEmails = data.ReplyToEmails;

            // 更新关联的项
            foreach (var targetId in data._sendingTargetIds)
            {
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
            return this.SentTotalToday >= this.MaxSendCountPerDay;
        }

        /// <summary>
        /// 是否包含指定的发件组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public bool ContainsSendingGroup(long sendingGroupId)
        {
            return _sendingTargetIds.Select(x => x.SendingGroupId).Contains(sendingGroupId);
        }

        /// <summary>
        /// 获取发件组 id
        /// </summary>
        /// <returns></returns>
        public List<long> GetSendingGroupIds()
        {
            return _sendingTargetIds.Select(x => x.SendingGroupId).ToList();
        }

        /// <summary>
        /// 获取指定了发件箱的邮件
        /// </summary>
        /// <returns></returns>
        public List<long> GetSpecificSendingItemIds()
        {
            return _sendingTargetIds
                .Where(x => x.SendingGroupId > 0)
                .Select(x => x.SendingItemId)
                .ToList();
        }

        /// <summary>
        /// 移除指定的发件项
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <param name="sendingItemId"></param>
        public void RemoveSepecificSendingItem(long sendingGroupId, long sendingItemId)
        {
            _sendingTargetIds.Remove(new SendingTargetId(sendingGroupId, sendingItemId));
        }

        /// <summary>
        /// 移除指定发送组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        public void RemoveSendingGroup(long sendingGroupId)
        {
            _sendingTargetIds = _sendingTargetIds
                .Where(x => x.SendingGroupId != sendingGroupId)
                .ToHashSet();
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

        private int _taskId = 0;

        public void SetTaskId(int taskId)
        {
            _taskId = taskId;
        }

        /// <summary>
        /// 是否正在任务中运行
        /// </summary>
        public bool IsRunningInTask => _taskId > 0;
        #endregion
    }
}
