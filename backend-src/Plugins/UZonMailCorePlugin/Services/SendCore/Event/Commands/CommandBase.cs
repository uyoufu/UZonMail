using UZonMail.CorePlugin.Services.SendCore.Contexts;

namespace UZonMail.CorePlugin.Services.SendCore.Event.Commands
{
    /// <summary>
    /// 事件中心参数
    /// 不要直接使用，所有类应继承 GenericEventArgs
    /// </summary>
    public abstract class CommandBase : EventArgs
    {
        public CommandBase(CommandType commandType, SendingContext scopeServices)
        {
            CommandType = commandType;
            ScopeServices = scopeServices;
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        public CommandType CommandType { get; }

        public SendingContext ScopeServices { get; }

        /// <summary>
        /// 上一级事件参数
        /// </summary>
        public CommandBase Parent { get; set; }

        /// <summary>
        /// 事件源
        /// A->B->C->A, A可以通过这个判断是否是自己发出的事件，防止循环调用
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// 判断事件是否来自自己
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool IsFromMe(object source)
        {
            if (Source.Equals(source))
                return true;
            if (!Parent.Equals(null))
                return Parent.IsFromMe(source);
            return false;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task Execute(object sender)
        {
            await EventCenter.Core.BroadCommand(sender, this);
        }
    }
}
