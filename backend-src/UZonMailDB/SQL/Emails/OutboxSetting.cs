using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Emails
{
    /// <summary>
    /// 发件箱设置
    /// </summary>
    public class OutboxSetting : SqlId
    {
        public string ProxyHost { get; set; }

        public string? ProxyPassword { get; set; }
    }
}
