using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Sender.Smtp;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender.MsGraph
{
    public class MsGraphClientFactory : ISingletonService
    {
        private ConcurrentDictionary<string, MsGraphClient> _clients = new();

        /// <summary>
        /// 获取 smtp 客户端
        /// 有可能更换了账号密码，要重新获取
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public async Task<Result<MsGraphClient>> GetMsGraphClientAsync(SendingContext sendingContext)
        {
           // 判断 MsGraphClient 是否过期，若过期，重新获取 accessToken

        }
    }
}
