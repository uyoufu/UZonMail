using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using UZonMail.CorePlugin.Services.Encrypt;

namespace UZonMail.CorePlugin.SignalRHubs
{
    /// <summary>
    /// 邮件发送进度 Hub
    /// </summary>
    public partial class UzonMailHub(EncryptService encryptService) : Hub<IUzonMailClient>
    {
        // 使用一个字典来跟踪用户的连接
        // 有可能同一个用户同时打开多个浏览器窗口
        // key: 连接 id
        // value: userId
        private static readonly ConcurrentDictionary<string, string> _userConnectionIds = new();

        /// <summary>
        /// 成功连接后
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // 当用户连接时，将他们添加到字典中
            string? userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnectionIds.TryAdd(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();

            // 发送欢迎消息
            await Clients.Caller.Notify(
                new Notify.NotifyMessage()
                {
                    Message = "欢迎使用 UzonMail !",
                    Type = Notify.NotifyType.Success
                }
            );
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // 当用户断开连接时，将他们从字典中移除
            _userConnectionIds.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
