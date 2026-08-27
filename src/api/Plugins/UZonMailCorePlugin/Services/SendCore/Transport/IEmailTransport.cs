using MimeKit;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Transport;

public interface IEmailTransport : ISingletonService<IEmailTransport>
{
    SendingProtocol Type { get; }

    Task<TransportResult> SendAsync(
        SendingContext context,
        MimeMessage message,
        CancellationToken cancellationToken = default
    );

    Task<TransportResult> ValidateAsync(
        IServiceProvider scopeServiceProvider,
        SenderEmailAddress senderAccount,
        CancellationToken cancellationToken = default
    );
}

public interface ITransportFailureClassifier
{
    TransportResult Classify(Exception exception);
}
