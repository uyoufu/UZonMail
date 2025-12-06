using System.Collections.Concurrent;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Sender.MsGraph
{
    public class MsGraphClientFactory(IServiceProvider serviceProvider) : ISingletonService
    {
        /// <summary>
        /// Graph 客户端工厂
        /// key: 发件箱地址
        /// </summary>
        private readonly ConcurrentDictionary<string, MsGraphClient> _clients = new();

        /// <summary>
        /// 获取 smtp 客户端
        /// 有可能更换了账号密码，要重新获取
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        public async Task<Result<MsGraphClient>> GetMsGraphClientAsync(
            SendingContext sendingContext
        )
        {
            var email = sendingContext.OutboxAddress!.Email;

            // 判断 MsGraphClient 是否过期，若过期，重新获取 accessToken
            if (!_clients.TryGetValue(email, out var client))
            {
                // 新建新的 client
                client = serviceProvider.GetRequiredService<MsGraphClient>();
                client.SetParams(email, 0);

                _clients.TryAdd(email, client);
            }
            return Result<MsGraphClient>.Success(client);
        }
    }
}
