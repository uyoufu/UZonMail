using Newtonsoft.Json.Linq;

namespace UzonMail.CorePlugin.Controllers.Emails.Models
{
    /// <summary>
    /// 发件项预览数据
    /// </summary>
    public class SendingItemPreview
    {
        /// <summary>
        /// 主题
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// 正文内容
        /// </summary>
        public required string Body { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public required JObject Data { get; set; }

        /// <summary>
        /// 收件联系人地址
        /// </summary>
        public required string RecipientContact { get; set; }

        /// <summary>
        /// 发件账户地址
        /// </summary>
        public required string SenderAccount { get; set; }
    }
}
