using System.ComponentModel.DataAnnotations.Schema;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Linq;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Templates;

namespace UzonMail.DB.SQL.Core.EmailSending
{
    /// <summary>
    /// 发件组
    /// 此处只记录统计数据
    /// 具体的数据由 EmailItem 记录
    /// </summary>
    public class SendingGroup : SqlId, IEntityTypeConfiguration<SendingGroup>
    {
        #region EF 定义
        public SendingGroupSourceType SourceType { get; set; } = SendingGroupSourceType.Manual;

        /// <summary>
        /// 用户名
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 主题
        /// 多个主题使用分号或者换行分隔
        /// </summary>
        public string Subjects { get; set; } = string.Empty;

        /// <summary>
        /// 模板
        /// 不包含用户中的模板
        /// </summary>
        public List<EmailTemplate>? Templates { get; set; } = [];

        /// <summary>
        /// 正文内容
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// 发件账户
        /// 不包含数据中的发件账户
        /// </summary>
        public List<SenderAccount> SenderAccounts { get; set; } = [];

        /// <summary>
        /// 发件账户组
        /// </summary>
        [JsonField]
        public List<IdAndName>? SenderAccountGroups { get; set; } = [];

        /// <summary>
        /// 所有发件账户的数量
        /// </summary>
        public int SenderAccountCount { get; set; }

        #region 用于前端传递参数
        /// <summary>
        /// 收件联系人
        /// </summary>
        [JsonField]
        public List<EmailAddress> Recipients { get; set; } = [];

        /// <summary>
        /// 收件联系人组
        /// </summary>
        [JsonField]
        public List<IdAndName>? RecipientContactGroups { get; set; } = [];

        /// <summary>
        /// 所有收件联系人的数量
        /// </summary>
        public int RecipientCount { get; set; }

        /// <summary>
        /// 抄送箱
        /// </summary>
        [JsonField]
        public List<EmailAddress>? CcBoxes { get; set; } = [];

        /// <summary>
        /// 密送
        /// </summary>
        [JsonField]
        public List<EmailAddress>? BccBoxes { get; set; } = [];
        #endregion

        /// <summary>
        /// 附件
        /// </summary>
        public List<FileUsage>? Attachments { get; set; } = [];

        /// <summary>
        /// 用户通过 excel 上传的数据
        /// </summary>
        [JsonField]
        public JArray? Data { get; set; }

        /// <summary>
        /// 是否分布式发件
        /// </summary>
        public bool IsDistributed { get; set; }

        /// <summary>
        /// 总发件数量
        /// Recipients 的数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 成功的数量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 已经发送的数量
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public SendingGroupStatus Status { get; set; }

        /// <summary>
        /// 当前运行状态的业务原因。
        /// </summary>
        public string? StatusReason { get; set; }

        /// <summary>
        /// 等待状态预计恢复的 UTC 时间。
        /// </summary>
        public DateTime? ResumeAtUtc { get; set; }

        /// <summary>
        /// 发送开始时间
        /// </summary>
        public DateTime SendStartDate { get; set; }

        /// <summary>
        /// 发送结束时间
        /// </summary>
        public DateTime SendEndDate { get; set; }

        /// <summary>
        /// 最后一条邮件的消息
        /// </summary>
        public string? LastMessage { get; set; }

        #region 定时发件相关
        /// <summary>
        /// 发件类型
        /// </summary>
        public SendingGroupType SendingType { get; set; }

        /// <summary>
        /// 定时发件时间
        /// </summary>
        public DateTime ScheduleDate { get; set; }
        #endregion

        /// <summary>
        /// 使用到的代理
        /// </summary>
        [JsonField]
        public List<long>? ProxyIds { get; set; } = [];
        #endregion

        #region 临时数据，不保存到数据库
        /// <summary>
        /// 批量改善
        /// </summary>
        [NotMapped]
        public bool SendBatch { get; set; }
        #endregion

        #region 外部工具方法
        private List<string>? _subjects;
        private static readonly string[] separators = ["\r\n", "\n", ";", "；"];

        public List<string> SplitSubjects()
        {
            if (_subjects == null)
            {
                // 说明没有初始化
                if (string.IsNullOrEmpty(Subjects))
                {
                    _subjects = [string.Empty];
                    return _subjects;
                }

                // 分割主题
                _subjects = [.. Subjects.Split(separators, StringSplitOptions.RemoveEmptyEntries)];
            }
            return _subjects;
        }

        /// <summary>
        /// 若有多个主题，则获取随机主题
        /// </summary>
        /// <returns></returns>
        public string GetRandSubject()
        {
            var subjects = SplitSubjects();
            // 返回随机主题
            return subjects[new Random().Next(subjects.Count)];
        }

        /// <summary>
        /// 获取第一个主题
        /// </summary>
        /// <returns></returns>
        public string GetFirstSubject()
        {
            return SplitSubjects().FirstOrDefault() ?? string.Empty;
        }

        #endregion

        public void Configure(EntityTypeBuilder<SendingGroup> builder)
        {
            builder.HasIndex(x => new
            {
                x.Status,
                x.ResumeAtUtc,
                x.Id
            });
            builder.HasMany(x => x.Templates).WithMany();
            builder.HasMany(x => x.Attachments).WithMany();
            builder.HasMany(x => x.SenderAccounts).WithMany();
        }
    }
}
