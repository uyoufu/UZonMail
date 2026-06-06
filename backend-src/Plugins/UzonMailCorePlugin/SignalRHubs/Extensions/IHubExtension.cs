using Microsoft.AspNetCore.SignalR;

namespace UZonMail.CorePlugin.SignalRHubs.Extensions
{
    public static class IHubExtension
    {
        /// <summary>
        /// 获取 user 端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hub"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static T GetUserClient<THub, T>(this IHubContext<THub, T> context, long userId)
            where THub : Hub<T>
            where T : class
        {
            return context.Clients.User(userId.ToString());
        }
    }
}
