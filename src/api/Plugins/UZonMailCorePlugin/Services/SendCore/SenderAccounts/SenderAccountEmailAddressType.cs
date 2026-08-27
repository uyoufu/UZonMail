namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts
{
    /// <summary>
    /// 发件箱地址类型
    /// 可以同时共存在
    /// 若共存在，则优先发特定收件地址的邮件
    /// </summary>
    [Flags]
    public enum SenderEmailAddressType
    {
        /// <summary>
        /// 未绑定到当前发送组。
        /// </summary>
        None = 0,

        /// <summary>
        /// 指定收件箱
        /// </summary>
        Specific = 1 << 0,

        /// <summary>
        /// 共享的
        /// </summary>
        Shared = 1 << 1,
    }
}
