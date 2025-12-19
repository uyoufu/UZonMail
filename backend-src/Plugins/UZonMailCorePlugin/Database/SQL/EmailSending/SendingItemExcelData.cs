using Newtonsoft.Json.Linq;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Exceptions;

namespace UZonMail.CorePlugin.Database.SQL.EmailSending
{
    /// <summary>
    /// 发件组中的 Excel 数据
    /// Excel 中的数据会覆盖 SendingGroup 中的数据
    /// </summary>
    public class SendingItemExcelData : JObject
    {
        public SendingItemExcelData() { }

        public SendingItemExcelData(JObject? row)
            : base(row ?? [])
        {
            // 提取数据
            if (row == null)
                return;

            OutboxId = row.SelectTokenOrDefault("outboxId", 0L);
            Outbox = row.SelectTokenOrDefault("outbox", string.Empty);
            OutboxName = row.SelectTokenOrDefault("outboxName", string.Empty);
            Inbox = row.SelectTokenOrDefault("inbox", string.Empty);
            InboxName = row.SelectTokenOrDefault("inboxName", string.Empty);
            Subject = row.SelectTokenOrDefault("subject", string.Empty);
            CC = row.SelectTokenOrDefault("cc", string.Empty)
                .SplitBySeparators()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            BCC = row.SelectTokenOrDefault("bcc", string.Empty)
                .SplitBySeparators()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            TemplateName = row.SelectTokenOrDefault("templateName", string.Empty);
            TemplateId = row.SelectTokenOrDefault("templateId", 0L);
            Body = row.SelectTokenOrDefault("body", string.Empty);
            ProxyId = row.SelectTokenOrDefault("proxyId", 0L);
            AttachmentNames = row.SelectTokenOrDefault("attachmentNames", string.Empty)
                .SplitBySeparators()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            // 其它的数据为用户自定义数据
            // 验证 cc 和 bcc 是否符合邮箱格式
            foreach (var cc in CC)
            {
                if (!cc.IsEmail())
                {
                    throw new KnownException($"抄送邮箱格式不正确: {cc}");
                }
            }

            foreach (var bcc in BCC)
            {
                if (!bcc.IsEmail())
                {
                    throw new KnownException($"密送邮箱格式不正确: {bcc}");
                }
            }
        }

        // 发件箱
        public long OutboxId { get; private set; }
        public string? Outbox { get; private set; }
        public string? OutboxName { get; private set; }

        // 发件箱
        public string? Inbox { get; private set; }
        public string? InboxName { get; private set; }

        // 主题
        public string? Subject { get; private set; }
        public List<string>? CC { get; private set; }
        public List<string>? BCC { get; private set; }

        // 模板，两者选其一即可，优先 templateId
        public string? TemplateName { get; private set; }
        public long TemplateId { get; private set; }

        /// <summary>
        /// 正文内容
        /// </summary>
        public string? Body { get; private set; }

        // 代理
        public long ProxyId { get; private set; }

        /// <summary>
        /// 附件名称
        /// </summary>
        public List<string> AttachmentNames { get; private set; }
    }
}
