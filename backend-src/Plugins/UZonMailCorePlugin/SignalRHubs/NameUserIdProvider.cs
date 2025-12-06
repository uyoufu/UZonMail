using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace UZonMail.CorePlugin.SignalRHubs
{
    /// <summary>
    /// SignalR 用户 ID 提供者
    /// </summary>
    public class NameUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            if (connection.User?.Identity is not ClaimsIdentity claims)
                return null;
            var name = claims.FindFirst(ClaimTypes.Name)?.Value;
            return name;
        }
    }
}
