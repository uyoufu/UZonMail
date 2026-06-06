using UZonMail.CorePlugin.Services.SendCore.Contexts;

namespace UZonMail.CorePlugin.Services.SendCore.Event.Commands
{
    /// <summary>
    /// 发件结束后的参数
    /// </summary>
    public class SendingTaskEndCommand : GenericCommand<long>
    {
        public SendingTaskEndCommand(SendingContext scopeServices, long userId)
            : base(CommandType.SendingTaskDisposed, scopeServices, userId)
        {
            UserId = userId;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public long UserId { get; private set; }

        /// <summary>
        /// 发件组 Id
        /// </summary>
        public long SendingGroupId { get; set; }
    }
}
