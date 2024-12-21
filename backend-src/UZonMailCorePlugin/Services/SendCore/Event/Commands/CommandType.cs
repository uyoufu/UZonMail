namespace UZonMail.Core.Services.SendCore.Event.Commands
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum CommandType
    {
        None,

        /// <summary>
        /// 开始发送
        /// </summary>
        StartSending,

        /// <summary>
        /// 发件任务被释放
        /// </summary>
        SendingTaskDisposed,

        /// <summary>
        /// 释放用户的发件箱池
        /// </summary>
        DisposeUserOutboxPoolCommand,

        /// <summary>
        /// 添加发件组 Id 到发件箱中
        /// </summary>
        AddSendingGroupIdToOutbox22,
    }
}
