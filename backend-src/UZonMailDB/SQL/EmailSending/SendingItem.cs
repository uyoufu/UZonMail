using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Files;

namespace UZonMail.DB.SQL.EmailSending
{
    /// <summary>
    /// 邮件项
    /// 实体配置参考：https://learn.microsoft.com/zh-cn/ef/core/modeling/#grouping-configuration
    /// </summary>
    //[EntityTypeConfiguration(typeof(SendingItem))] 不再需要，因为在 EntityTypeConfig 中已经配置了自动加载
    public class SendingItem : OrgId, IEntityTypeConfiguration<SendingItem>
    {
        #region EF 定义
        /// <summary>
        /// 所属发送任务
        /// </summary>
        public long SendingGroupId { get; set; }
        public SendingGroup SendingGroup { get; set; }

        /// <summary>
        /// 所属用户
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 发件人
        /// 由于是多线程发件，这个值只有发送后才能确定
        /// 若一开始由数据指定，则其值 > 0
        /// </summary>
        public long OutBoxId { get; set; }

        /// <summary>
        /// 实际发件人
        /// 由于是多线程发件，这个值只有发送后才能确定
        /// </summary>
        public string? FromEmail { get; set; }

        /// <summary>
        /// 收件人
        /// 可能有多个收件人
        /// </summary>
        [JsonField]
        public List<EmailAddress> Inboxes { get; set; }
        /// <summary>
        /// 抄送人
        /// </summary>
        [JsonField]
        public List<EmailAddress>? CC { get; set; }
        /// <summary>
        /// 密送人
        /// </summary>
        [JsonField]
        public List<EmailAddress>? BCC { get; set; }

        /// <summary>
        /// 收件人、抄送人、密送人的邮箱地址，使用逗号分隔
        /// 方便查询
        /// </summary>
        public string? ToEmails { get; set; }

        /// <summary>
        /// 邮件模板 Id
        /// 可以为 0，表示不使用模板
        /// 模板是发送时，动态指定的
        /// 若一开始由数据指定，则其值 > 0
        /// </summary>
        public long EmailTemplateId { get; set; }

        /// <summary>
        /// 发送主题
        /// 由于可能是随机主题，因此要记录
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// 实际发送内容
        /// 将模板与数据进行合并后的内容
        /// 若初始时不为空，则说明是被重写了
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// 附件的 FileUsageId
        /// </summary>
        public List<FileUsage>? Attachments { get; set; } = [];

        /// <summary>
        /// 批量发送
        /// </summary>
        public bool IsSendingBatch { get; set; }

        /// <summary>
        /// 发送代理
        /// </summary>
        public long ProxyId { get; set; }

        [JsonField]
        public JObject? Data { get; set; }

        /// <summary>
        /// 是否启用邮件跟踪器
        /// </summary>
        public bool EnableEmailTracker { get; set; }
        #endregion

        #region 发送结果
        /// <summary>
        /// 发送日期
        /// </summary>
        public DateTime SendDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public SendingItemStatus Status { get; set; }

        /// <summary>
        /// 发送结果
        /// </summary>
        public string? SendResult { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int TriedCount { get; set; }

        /// <summary>
        /// Smpt 服务器返回的 Id
        /// 通过这个 id 去获取阅读状态
        /// </summary>
        public string? ReceiptId { get; set; }

        /// <summary>
        /// 已读时间
        /// </summary>
        public DateTime ReadDate { get; set; }
        #endregion

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<SendingItem> builder)
        {
            builder.HasMany(x => x.Attachments).WithMany();
        }
    }
}
