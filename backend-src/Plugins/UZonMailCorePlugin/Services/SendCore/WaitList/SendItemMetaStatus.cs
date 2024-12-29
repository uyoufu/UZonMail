namespace UZonMail.Core.Services.SendCore.WaitList
{
    /// <summary>
    /// 发件项元数据状态
    /// </summary>
    public enum SendItemMetaStatus
    {
        /// <summary>
        /// 等待发件
        /// </summary>
        Pending = 1,

        /// <summary>
        /// 进行中
        /// </summary>
        Working,

        /// <summary>
        /// 可以释放
        /// </summary>
        Error,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 已经释放
        /// </summary>
        Disposed,
    }
}
