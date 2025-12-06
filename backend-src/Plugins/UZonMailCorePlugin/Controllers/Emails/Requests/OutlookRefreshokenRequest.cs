using UZonMail.CorePlugin.Config.SubConfigs;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Controllers.Emails.Requests
{
    /// <summary>
    /// Outlook 刷新令牌请求
    /// </summary>
    public class OutlookRefreshokenRequest : FluentHttpRequest, ITransientService
    {
        private string _baseUrl = string.Empty;

        public OutlookRefreshokenRequest(HttpClient httpClient, IConfiguration configuration)
        {
            WithMethod(HttpMethod.Post);
            WithHttpClient(httpClient);
            WithUrl("https://login.microsoftonline.com/common/oauth2/v2.0/token");

            _baseUrl = configuration.GetValue<string>("BaseUrl")!;
        }

        public OutlookRefreshokenRequest WithFormData(
            string clientId,
            string? clientSecret,
            string code
        )
        {
            var formData = new Dictionary<string, string>()
            {
                { "client_id", clientId },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", $"{_baseUrl.Trim('/')}/api/v1/outlook-authorization/code" },
                { "scope", string.Join(" ", OutlookAuthorizationRequest.SendScopes) }
            };
            if (!string.IsNullOrEmpty(clientSecret))
            {
                formData.Add("client_secret", clientSecret);
            }

            WithFormContent(formData);
            return this;
        }
    }
}
