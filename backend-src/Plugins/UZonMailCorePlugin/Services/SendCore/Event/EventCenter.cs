using System;
using System.Threading.Tasks;
using UZonMail.Core.Services.SendCore.Event.Commands;

namespace UZonMail.Core.Services.SendCore.Event
{
    /// <summary>
    /// 事件中心
    /// 使用约定：
    /// 1. 事件中尽量不再触发事件，防止循环调用
    /// 2. 事件应在投入使用时才开始注册，少在在初始化函数中注册
    /// 3. 不要在非常细粒度的对象中注册事件，减少事件量
    /// 4. 事件类应尽量详细
    /// </summary>
    public class EventCenter
    {
        private EventCenter() { }
        private static readonly Lazy<EventCenter> _instance = new(() => new EventCenter());
        /// <summary>
        /// 事件中心的实例
        /// 该实例是线程安全的
        /// </summary>
        public static EventCenter Core => _instance.Value;

        private readonly AsyncWeakEvent<CommandBase> _commands = new();

        /// <summary>
        /// 数据变更事件
        /// </summary>
        public event Func<object?, CommandBase, Task> DataChanged
        {
            add => _commands.Add(value, value.Invoke);
            remove => _commands.Remove(value);
        }

        /// <summary>
        /// 广播命令
        /// </summary>
        /// <param name="args"></param>
        public async Task BroadCommand(object sender, CommandBase args)
        {
            await _commands.InvokeAsync(sender, args);
        }
    }
}
