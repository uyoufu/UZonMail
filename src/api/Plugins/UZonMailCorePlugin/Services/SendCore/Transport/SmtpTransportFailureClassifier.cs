using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Transport;

public sealed class SmtpTransportFailureClassifier
    : ITransportFailureClassifier,
        ISingletonService<ITransportFailureClassifier>
{
    private static readonly HashSet<int> PermanentAuthenticationCodes = [432, 530, 534, 535, 538];

    public TransportResult Classify(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException
                => TransportResult.Failure(SendFailureKind.Cancelled, exception.Message),
            SmtpCommandException smtp => ClassifySmtp(smtp),
            AuthenticationException
            or System.Security.Authentication.AuthenticationException
            or SslHandshakeException
                => TransportResult.Failure(
                    SendFailureKind.SenderAccountPermanent,
                    exception.Message,
                    errorCode: exception.GetType().Name
                ),
            SocketException
            or IOException
            or TimeoutException
            or ServiceNotConnectedException
            or ServiceNotAuthenticatedException
                => TransportResult.Failure(
                    SendFailureKind.Network,
                    exception.Message,
                    errorCode: exception.GetType().Name
                ),
            _
                => TransportResult.Failure(
                    SendFailureKind.Unknown,
                    exception.Message,
                    errorCode: exception.GetType().Name
                ),
        };
    }

    private static TransportResult ClassifySmtp(SmtpCommandException exception)
    {
        var statusCode = (int)exception.StatusCode;
        var kind = statusCode switch
        {
            _ when PermanentAuthenticationCodes.Contains(statusCode)
                => SendFailureKind.SenderAccountPermanent,
            >= 400 and < 500 => SendFailureKind.Transient,
            _ when IsHardBounce(exception) => SendFailureKind.HardBounce,
            >= 500 when exception.ErrorCode == SmtpErrorCode.RecipientNotAccepted
                => SendFailureKind.RecipientPermanent,
            >= 500 when exception.ErrorCode == SmtpErrorCode.MessageNotAccepted
                => SendFailureKind.MessagePermanent,
            >= 500 => SendFailureKind.SenderAccountPermanent,
            _ => SendFailureKind.Unknown,
        };

        return TransportResult.Failure(
            kind,
            exception.Message,
            statusCode,
            exception.ErrorCode.ToString(),
            exception.Mailbox?.Address
        );
    }

    private static bool IsHardBounce(SmtpCommandException exception)
    {
        if (exception.ErrorCode != SmtpErrorCode.RecipientNotAccepted || exception.Mailbox == null)
            return false;

        return (int)exception.StatusCode is 550 or 551 or 553;
    }
}
