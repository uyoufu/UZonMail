using System.ComponentModel.DataAnnotations.Schema;
using UZonMail.Core.Services.Settings.Core;
using UZonMail.Utils.Extensions;

namespace UZonMail.Core.Services.Settings.Model
{
    /// <summary>
    /// 发送设置
    /// </summary>
    public class SendingSetting : BaseSettingModel
    {
        /// <summary>
        /// 每日每个发件箱最大发送次数
        /// 为 0 时表示不限制
        /// </summary>
        public int MaxSendCountPerEmailDay { get; set; } = 0;

        /// <summary>
        /// 最小发件箱冷却时间
        /// </summary>
        public int MinOutboxCooldownSecond { get; set; } = 5;

        /// <summary>
        /// 最大发件箱冷却时间
        /// </summary>
        public int MaxOutboxCooldownSecond { get; set; } = 10;

        /// <summary>
        /// 最大批量发件数
        /// </summary>
        public int MaxSendingBatchSize { get; set; } = 20;

        /// <summary>
        /// 收件箱最小收件间隔时间，单位小时
        /// </summary>
        public int MinInboxCooldownHours { get; set; } = -1;

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
        /// 回复邮件地址列表
        /// </summary>
        [NotMapped]
        public List<string> ReplyToEmailsList
        {
            get
            {
                return ReplyToEmails.SplitBySeparators().Distinct().ToList();
            }
        }

        /// <summary>
        /// 获取冷却时间
        /// 随机
        /// </summary>
        /// <returns></returns>
        public int GetCooldownMilliseconds()
        {
            var min = Math.Max(0, MinOutboxCooldownSecond);
            var max = Math.Max(0, MaxOutboxCooldownSecond);
            if (max <= min)
            {
                return min * 1000;
            }

            // 随机从 min 到 max 取值
            return new Random().Next(min, max) * 1000;
        }

        protected override void InitValue()
        {
            MaxSendCountPerEmailDay = GetIntValue(nameof(MaxSendCountPerEmailDay), 0);
            MinOutboxCooldownSecond = GetIntValue(nameof(MinOutboxCooldownSecond), 5);
            MaxSendingBatchSize = GetIntValue(nameof(MaxSendingBatchSize), 20);
            MinInboxCooldownHours = GetIntValue(nameof(MinInboxCooldownHours), 0);
            ReplyToEmails = GetStringValue(nameof(ReplyToEmails), string.Empty);
            MaxRetryCount = GetIntValue(nameof(MaxRetryCount), 3);           
            ChangeIpAfterEmailCount = GetIntValue(nameof(ChangeIpAfterEmailCount), 0);
        }
    }
}
