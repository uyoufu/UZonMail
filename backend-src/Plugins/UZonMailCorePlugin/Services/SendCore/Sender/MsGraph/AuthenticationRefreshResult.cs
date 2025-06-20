using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace UZonMail.Core.Services.SendCore.Sender.MsGraph
{
    public class AuthenticationRefreshResult
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
        [JsonProperty("ext_expires_in")]
        public string ExtExPiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// 转换成 AuthenticationResult 对象
        /// </summary>
        /// <returns></returns>
        public AuthenticationResult ToAuthenticationResult()
        {

        }
    }
}
