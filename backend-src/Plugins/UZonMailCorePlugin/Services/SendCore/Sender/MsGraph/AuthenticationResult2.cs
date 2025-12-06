using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace UZonMail.CorePlugin.Services.SendCore.Sender.MsGraph
{
    /// <summary>
    /// 当前程序专业的 OAuth2 认证结果类
    /// </summary>
    public class AuthenticationResult2
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("expires_in")]
        public double ExpiresIn { get; set; }

        [JsonProperty("ext_expires_in")]
        [Obsolete("微软已弃用")]
        public double ExtExPiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        private DateTime _expireAt = DateTime.MinValue;

        /// <summary>
        /// 在某个时间点过期
        /// </summary>
        public DateTime ExpireAt
        {
            get
            {
                if (_expireAt == DateTime.MinValue)
                {
                    // 防止判断过期时，刚好在过期时间点
                    _expireAt = DateTime.UtcNow.AddSeconds(ExpiresIn - 10);
                }

                return _expireAt;
            }
        }

        public bool IsPersonalAccount { get; private set; } = true;

        /// <summary>
        /// 转换成 AuthenticationResult 对象
        /// </summary>
        /// <returns></returns>
        public static AuthenticationResult2 FromAuthenticationResult(
            AuthenticationResult authenticationResult
        )
        {
            return new AuthenticationResult2()
            {
                AccessToken = authenticationResult.AccessToken,
                Scope = string.Join(" ", authenticationResult.Scopes),
                TokenType = authenticationResult.TokenType,
                ExpiresIn = (authenticationResult.ExpiresOn - DateTimeOffset.UtcNow).TotalSeconds,
                ExtExPiresIn = (
                    authenticationResult.ExtendedExpiresOn - DateTimeOffset.UtcNow
                ).TotalSeconds,
                IsPersonalAccount = false
            };
        }
    }
}
