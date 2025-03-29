using System.ComponentModel.DataAnnotations.Schema;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Settings
{
    public class OrganizationSetting : OrgId
    {
        /// <summary>
        /// 优先级
        /// 优先级大的覆盖小的
        /// 同等优先级，按选择顺序进行覆盖，后者覆盖前者
        /// </summary>
        public int Priority { get; set; }        

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
        public int MinInboxCooldownHours { get; set; }

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
        /// 发送邮件跟踪器
        /// </summary>
        public bool? EnableEmailTracker { get; set; }

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
            if(max<= min)
            {
                return min * 1000;
            }

            // 随机从 min 到 max 取值
            return new Random().Next(min, max) * 1000;
        }
    }
}
