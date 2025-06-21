using UZonMail.Utils.Http.Request;

namespace UZonMail.Core.Controllers.Emails.Requests
{
    /// <summary>
    /// 参考: https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp#client-credentials-provider
    /// 个人邮箱限制
    /// 不支持应用程序权限（Application Permissions）
    /// 不支持客户端凭据流（Client Credentials Flow）
    /// 只支持委托权限（Delegated Permissions
    /// </summary>
    public class OutlookAuthorizationRequest : FluentHttpRequest
    {
        public OutlookAuthorizationRequest()
        {
            WithUrl("https://login.microsoftonline.com/common");
        }
    }
}
