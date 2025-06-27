using Microsoft.AspNetCore.Authorization;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Controllers.Emails.Requests
{
    /// <summary>
    /// 参考: https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp#client-credentials-provider
    /// 个人邮箱限制
    /// 不支持应用程序权限（Application Permissions）
    /// 不支持客户端凭据流（Client Credentials Flow）
    /// 只支持委托权限（Delegated Permissions
    /// </summary>
    public class OutlookAuthorizationRequest : FluentHttpRequest, ITransientService
    {
        private static readonly List<string> _sendScopes = [
                "offline_access",
                "Mail.Send", //  替代 SMTP 发件权限 
                //"Mail.Read"  // 替代 IMAP 收件权限
            ];

        public OutlookAuthorizationRequest(HttpClient httpClient, IConfiguration configuration)
        {
            var baseUrl = configuration.GetValue<string>("BaseUrl");
            if(string.IsNullOrEmpty(baseUrl))
            {
               throw new KnownException("BaseUrl 配置不能为空，请检查配置文件。");
            }

            WithHttpClient(httpClient);
            WithUrl("https://login.microsoftonline.com/common/oauth2/v2.0/authorize");
            AddQuery("redirect_uri", $"{baseUrl.Trim('/')}/api/v1/outlook-authorization/code");
            AddQuery("response_type", "code");
            AddQuery("response_mode", "query");
            AddQuery("scope", string.Join(" ", _sendScopes));
        }

        public OutlookAuthorizationRequest WithClientId(string clientId)
        {
            AddQuery("client_id", clientId);
            return this;
        }

        /// <summary>
        /// 通过 state 进行回调
        /// </summary>
        /// <param name="outboxId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public OutlookAuthorizationRequest WithState(long outboxId)
        {
            AddQuery("state", outboxId.ToString());
            return this;
        }
    }
}
