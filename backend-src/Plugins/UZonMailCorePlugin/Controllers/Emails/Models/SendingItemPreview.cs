using Newtonsoft.Json.Linq;

namespace UZonMail.Core.Controllers.Emails.Models
{
    /// <summary>
    /// 发件项预览数据
    /// </summary>
    public class SendingItemPreview
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 正文内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public JObject Data { get; set; }

        /// <summary>
        /// 收件箱
        /// </summary>
        public string Inbox { get; set; }

        /// <summary>
        /// 发件箱
        /// </summary>
        public string Outbox { get; set; }
    }
}
