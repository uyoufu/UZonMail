namespace UZonMail.CorePlugin.SignalRHubs.Notify
{
    public class NotifyMessage
    {
        public string? Message { get; set; }

        /// <summary>
        /// 类型有：info, success, warning, error
        /// </summary>
        public string Type { get; set; } = NotifyType.success.ToString();

        public string? Title { get; set; }
    }

    public enum NotifyType
    {
        info,
        success,
        warning,
        error
    }
}
