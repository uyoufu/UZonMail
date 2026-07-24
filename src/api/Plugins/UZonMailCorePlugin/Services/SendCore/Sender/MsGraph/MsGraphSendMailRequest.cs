using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using MimeKit;
using UzonMail.Utils.Http.Request;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph
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
            Content = new MimeBase64Content(mimeMessage);
            return this;
        }

        private sealed class MimeBase64Content : HttpContent
        {
            public MimeBase64Content(MimeMessage message)
            {
                _message = message;
                Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            }

            private readonly MimeMessage _message;

            protected override async Task SerializeToStreamAsync(
                Stream stream,
                TransportContext? context
            )
            {
                using var transform = new ToBase64Transform();
                await using var encoded = new CryptoStream(
                    stream,
                    transform,
                    CryptoStreamMode.Write,
                    leaveOpen: true
                );
                await _message.WriteToAsync(encoded);
                encoded.FlushFinalBlock();
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }
    }
}
