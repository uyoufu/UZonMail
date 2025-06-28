using MimeKit;
using UZonMail.Utils.Http.Request;

namespace UZonMail.Core.Services.SendCore.Sender.MsGraph
{
    public class MsGraphSendMailRequest : FluentHttpRequest
    {
        public MsGraphSendMailRequest()
        {
            WithMethod(HttpMethod.Post);
        }

        public MsGraphSendMailRequest WithAccessToken(string accessToken)
        {
            AddHeader("Authorization", $"Bearer {accessToken}");
            return this;
        }

        public MsGraphSendMailRequest WithMimeMessage(MimeMessage mimeMessage)
        {
            using var ms = new MemoryStream();
            mimeMessage.WriteTo(ms);
            ms.Position = 0;

            var base64 = Convert.ToBase64String(ms.ToArray());
            Content = new StringContent(base64, System.Text.Encoding.UTF8, "text/plain");
            return this;
        }
    }
}
