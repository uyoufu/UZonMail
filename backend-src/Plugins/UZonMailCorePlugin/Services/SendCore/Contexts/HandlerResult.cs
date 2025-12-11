namespace UZonMail.CorePlugin.Services.SendCore.Contexts
{
    public class HandlerResult(
        HandlerStatus handlerStatus,
        string message = "",
        ChainStatus chainStatus = ChainStatus.Normal
    ) : IHandlerResult
    {
        public HandlerStatus HandlerStatus => handlerStatus;

        public ChainStatus ChainStatus => chainStatus;

        public string Message => message;

        #region 静态方法
        public static IHandlerResult Normal(
            string message = "",
            ChainStatus chainStatus = ChainStatus.Normal
        ) => new HandlerResult(HandlerStatus.Normal, message, chainStatus);

        /// <summary>
        /// 生成一个默认的跳过结果
        /// 只有当结果不会对后续处理产生影响时，才使用跳过结果
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chainStatus"></param>
        /// <returns></returns>
        public static IHandlerResult Skiped(
            string message = "",
            ChainStatus chainStatus = ChainStatus.Normal
        ) => new HandlerResult(HandlerStatus.Skiped, message, chainStatus);

        public static IHandlerResult Success(
            string message = "",
            ChainStatus chainStatus = ChainStatus.Normal
        ) => new HandlerResult(HandlerStatus.Success, message, chainStatus);

        /// <summary>
        /// 生成一个失败结果
        /// 当结果会对后续处理产生影响时，使用失败结果
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chainStatus"></param>
        /// <returns></returns>
        public static IHandlerResult Failed(
            string message = "",
            ChainStatus chainStatus = ChainStatus.Normal
        ) => new HandlerResult(HandlerStatus.Failed, message, chainStatus);

        #endregion
    }
}
