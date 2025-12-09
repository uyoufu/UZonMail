namespace UZonMail.CorePlugin.Services.SendCore.Contexts
{
    public enum ChainStatus
    {
        Normal = 1,

        /// <summary>
        /// 是否应该退出线程
        /// </summary>
        ShouldExitTask,

        /// <summary>
        /// 退出责任链
        /// 相当于只执行一个空的责任链方法
        /// </summary>
        BreakChain
    }
}
