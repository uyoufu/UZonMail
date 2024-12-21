
using UZonMail.Core.Services.SendCore.Contexts;

namespace UZonMail.Core.Services.SendCore.Event.Commands
{
    /// <summary>
    /// 释放用户的发件箱池
    /// </summary>
    public class DisposeUserOutboxPoolCommand : GenericCommand<long>
    {
        public DisposeUserOutboxPoolCommand(SendingContext scopeServices, long userId) : base(CommandType.DisposeUserOutboxPoolCommand, scopeServices, userId)
        {
            UserId = userId;
        }

        /// <summary>
        /// 用户 Id
        /// </summary>
        public long UserId { get; }
    }
}
