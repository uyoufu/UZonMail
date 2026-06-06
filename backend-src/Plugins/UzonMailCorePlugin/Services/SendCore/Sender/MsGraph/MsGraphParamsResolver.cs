using MailKit.Security;
using UZonMail.CorePlugin.Config.SubConfigs;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Sender.MsGraph
{
    /// <summary>
    /// 解析 Outbox 的 graph 设置
    /// 用户名有以下 2 种格式:
    /// 1. client_id
    /// 2. client_id/tenant_id
    ///
    /// 密码有以下 3 种格式:
    /// 1. refresh_token, 长度大于 80
    /// 2. client_secret
    /// 3. refresh_token/client_secret
    ///
    /// </summary>
    public class MsGraphParamsResolver : ITransientService
    {
        private MicrosoftEntraAppConfig _msEntraApp = new();

        public MsGraphParamsResolver(IConfiguration configuration)
        {
            configuration.GetRequiredSection("MicrosoftEntraApp").Bind(_msEntraApp);
        }

        /// <summary>
        /// 使用默认的 Microsoft Entra 应用程序
        /// </summary>
        private bool _useDefaultApp = false;

        public string ClientId { get; private set; }

        public string? TenantId { get; private set; }

        public string? ClientSecret { get; private set; }

        public string? RefreshToken { get; private set; }

        /// <summary>
        /// 是否存在 RefreshToken
        /// </summary>
        public bool HasRefreshToken => !string.IsNullOrEmpty(RefreshToken);

        /// <summary>
        /// 设置 Graph 的信息
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="plainPassword"></param>
        public void SetGraphInfo(string? userName, string? plainPassword = null)
        {
            // 用户名为空, 使用系统默认
            if (string.IsNullOrEmpty(userName))
            {
                _useDefaultApp = true;
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
            if (ClientId == _msEntraApp.ClientId)
            {
                TenantId = _msEntraApp.TenantId;
                ClientSecret = _msEntraApp.ClientSecret;
            }

            if (string.IsNullOrEmpty(plainPassword))
                return;

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
                throw new ArgumentException(
                    "密码格式不正确，应为 refresh_token 或 refresh_token/client_secret"
                );
            }
        }

        public void SetRefreshToken(string? refreshToken)
        {
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// 获取保存到数据库的 userName
        /// </summary>
        /// <returns></returns>
        public string GetUserNameForDB()
        {
            if (_useDefaultApp)
                return string.Empty;

            return string.IsNullOrEmpty(TenantId) ? ClientId : $"{ClientId}/{TenantId}";
        }

        /// <summary>
        /// 获取保存到数据库的密码
        /// </summary>
        /// <returns></returns>
        public string GetPasswordForDB()
        {
            var clientSecret = ClientSecret;
            // 如果是默认值，则不向前端暴露
            if (_useDefaultApp)
            {
                clientSecret = null;
            }

            var existsRefreshToken = !string.IsNullOrEmpty(RefreshToken);
            var existsClientSecret = !string.IsNullOrEmpty(clientSecret);

            if (existsRefreshToken && existsClientSecret)
            {
                return $"{RefreshToken}/{clientSecret}";
            }
            else if (existsRefreshToken)
            {
                return RefreshToken!;
            }
            else if (existsClientSecret)
            {
                return clientSecret!;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
