namespace UZonMail.CorePlugin.Services.SendCore.Contexts
{
    public enum HandlerStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 失败
        /// </summary>
        Failed,

        /// <summary>
        /// 跳过
        /// </summary>
        Skiped,

        /// <summary>
        /// 成功
        /// </summary>
        Success,
    }
}
