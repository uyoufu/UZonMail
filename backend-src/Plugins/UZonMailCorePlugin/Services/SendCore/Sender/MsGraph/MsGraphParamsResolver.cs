using UZonMail.Core.Config.SubConfigs;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender.MsGraph
{
    /// <summary>
    /// 解析 Outbox 的 graph 设置
    /// 用户名有以下 2 种格式:
    /// 1. client_id
    /// 2. client_id/tenant_id
    /// 
    /// 密码有以下 3 种格式:
    /// 1. refresh_token
    /// 2. client_secret
    /// 3. refresh_token/client_secret
    /// </summary>
    public class MsGraphParamsResolver : ITransientService
    {
        private MicrosoftEntraAppConfig _msEntraApp = new();

        public MsGraphParamsResolver(IConfiguration configuration)
        {
            configuration.GetRequiredSection("MicrosoftEntraApp").Bind(_msEntraApp);
        }


        public string ClientId { get;private set; }

        public string? TenantId { get; private set; }

        public string? ClientSecret { get;private set; }

        public string? RefreshToken { get; private set; }

        /// <summary>
        /// 设置 Graph 的信息
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="plainPassword"></param>
        public void SetGraphInfo(string? userName,string? plainPassword=null)
        {
            // 用户名为空, 使用系统默认
            if (string.IsNullOrEmpty(userName))
            {
                userName = _msEntraApp.ClientId;
            }

            // 解析用户名
            // 可能的情况:
            // 1. client_id
            // 2. client_id/tenant_id
            var userNameParts = userName.Split('/');
            if (userNameParts.Length == 1)
            {
                ClientId = userNameParts[0];
            }
            else if (userNameParts.Length == 2)
            {
                ClientId = userNameParts[0];
                TenantId = userNameParts[1];
            }
            else
            {
                throw new ArgumentException("用户名格式不正确，应为 client_id 或 client_id/tenant_id");
            }
            
            // 判断 clientId 是否为默认值
            if(ClientId==_msEntraApp.ClientId)
            {
                TenantId = _msEntraApp.TenantId;
                ClientSecret = _msEntraApp.ClientSecret;
                return;
            }

            if (string.IsNullOrEmpty(plainPassword)) return;

            // 解析密码
            // 可能的情况:
            // 1. refresh_token
            // 2. client_secret
            // 3. refresh_token/client_secret
            var passwordParts = plainPassword.Split('/');
            if (passwordParts.Length == 1)
            {
                // 根据长度和格式判断是否为 refresh_token
                if (passwordParts[0].Length < 20)
                {
                    throw new ArgumentException("密钥长度不正确");
                }
                if (passwordParts[0].Length < 80)
                {
                    // 当成是 client_secret
                    ClientSecret = passwordParts[0];
                }
                else
                {
                    RefreshToken = passwordParts[0];
                }
            }
            else if (passwordParts.Length == 2)
            {
                RefreshToken = passwordParts[0];
                ClientSecret = passwordParts[1];
            }
            else
            {
                throw new ArgumentException("密码格式不正确，应为 refresh_token 或 refresh_token/client_secret");
            }
        }
    }
}
