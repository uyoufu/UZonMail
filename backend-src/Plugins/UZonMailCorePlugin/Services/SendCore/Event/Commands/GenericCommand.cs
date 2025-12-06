using UZonMail.CorePlugin.Services.SendCore.Contexts;

namespace UZonMail.CorePlugin.Services.SendCore.Event.Commands
{
    /// <summary>
    /// 泛型事件参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericCommand<T> : CommandBase
    {
        public GenericCommand(CommandType eventType, SendingContext scopeServices, T? data)
            : base(eventType, scopeServices)
        {
            Data = data;
        }

        /// <summary>
        /// 附带的数据
        /// </summary>
        public T? Data { get; }
    }
}
