using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using UzonMail.CorePlugin.Utils.Cache;
using UzonMail.Utils.Extensions;

namespace UzonMail.CorePlugin.Services.Settings.Model
{
    /// <summary>
    /// 发送设置
    /// </summary>
    public class SendingSetting : BaseSettingModel
    {
        /// <summary>
        /// 每日每个发件账户最大发送次数
        /// 为 0 时表示不限制
        /// </summary>
        public int MaxSendCountPerEmailDay { get; set; } = 0;

        /// <summary>
        /// 最小发件账户冷却时间
        /// </summary>
        public int MinSenderAccountCooldownSecond { get; set; } = 5;

        /// <summary>
        /// 最大发件账户冷却时间
        /// </summary>
        public int MaxSenderAccountCooldownSecond { get; set; } = 10;

        /// <summary>
        /// 最大批量发件数
        /// </summary>
        public int MaxSendingBatchSize { get; set; } = 20;

        /// <summary>
        /// 收件联系人最小投递间隔时间，单位小时
        /// </summary>
        public int MinimumCooldownHours { get; set; } = -1;

        /// <summary>
        /// 回复的邮箱地址, 多个邮箱用逗号分隔
        /// </summary>
        public string? ReplyToEmails { get; set; }

        /// <summary>
        /// 最大重试次数
        /// 若为 0 则不重试
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 每 x 封邮件后，更换 IP
        /// 为 0 表示不更换
        /// </summary>
        public int ChangeIpAfterEmailCount { get; set; }

        /// <summary>
        /// 每小时每个 IP 最大发送次数
        /// </summary>
        public int MaxCountPerIPDomainHour { get; set; } = 0;

        /// <summary>
        /// 是否允许同一发件任务向同一收件人发送多封邮件
        /// </summary>
        public bool AllowDuplicateSending { get; set; }

        /// <summary>
        /// 回复邮件地址列表
        /// </summary>
        [NotMapped]
        public List<string> ReplyToEmailsList
        {
            get { return ReplyToEmails.SplitBySeparators().Distinct().ToList(); }
        }

        /// <summary>
        /// 获取冷却时间
        /// 随机
        /// </summary>
        /// <returns></returns>
        public int GetCooldownMilliseconds()
        {
            var min = Math.Max(0, MinSenderAccountCooldownSecond);
            var max = Math.Max(0, MaxSenderAccountCooldownSecond);
            if (max <= min)
            {
                return min * 1000;
            }
            // 随机从 min 到 max 取值
            int seconds = RandomNumberGenerator.GetInt32(min, max);
            return seconds * 1000;
        }

        protected override void ReadValuesFromJson()
        {
            MaxSendCountPerEmailDay = GetIntValue(nameof(MaxSendCountPerEmailDay), 0);
            MinSenderAccountCooldownSecond = GetIntValue(nameof(MinSenderAccountCooldownSecond), 5);
            MaxSenderAccountCooldownSecond = GetIntValue(
                nameof(MaxSenderAccountCooldownSecond),
                10
            );
            MaxSendingBatchSize = GetIntValue(nameof(MaxSendingBatchSize), 20);
            MinimumCooldownHours = GetIntValue(nameof(MinimumCooldownHours), 0);
            ReplyToEmails = GetStringValue(nameof(ReplyToEmails), string.Empty);
            // 0 明确表示不重试，负值按无效配置回退到默认值。
            var maxRetryCount = GetIntValue(nameof(MaxRetryCount), 3);
            MaxRetryCount = maxRetryCount < 0 ? 3 : maxRetryCount;
            ChangeIpAfterEmailCount = GetIntValue(nameof(ChangeIpAfterEmailCount), 0);
            MaxCountPerIPDomainHour = GetIntValue(nameof(MaxCountPerIPDomainHour), 1200);
            AllowDuplicateSending = GetBoolValue(nameof(AllowDuplicateSending), false);
        }
    }
}
