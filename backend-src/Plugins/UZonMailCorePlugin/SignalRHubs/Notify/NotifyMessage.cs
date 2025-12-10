namespace UZonMail.CorePlugin.SignalRHubs.Notify
{
    public class NotifyMessage
    {
        public string? Message { get; set; }

        /// <summary>
        /// 类型有：info, success, warning, error
        /// </summary>
        public NotifyType Type { get; set; } = NotifyType.Success;

        public string? Title { get; set; }
    }

    public enum NotifyType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
