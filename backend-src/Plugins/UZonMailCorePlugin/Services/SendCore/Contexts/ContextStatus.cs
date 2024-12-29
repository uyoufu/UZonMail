namespace UZonMail.Core.Services.SendCore.Contexts
{
    [Flags]
    public enum ContextStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 1 << 0,

        /// <summary>
        /// 失败
        /// </summary>
        Fail = 1 << 1,

        /// <summary>
        /// 成功
        /// </summary>
        Success = 1 << 2,

        /// <summary>
        /// 是否应该退出线程
        /// </summary>
        ShouldExitThread = 1 << 3,

        /// <summary>
        /// 退出责任链
        /// 相当于只执行一个空的责任链方法
        /// </summary>
        BreakChain = 1 << 4
    }
}
