using System.Net.Sockets;
using System.Security.Authentication;
using MailKit.Net.Smtp;
using MimeKit;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Transport;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SmtpTransportFailureClassifierTests
{
    private readonly SmtpTransportFailureClassifier _classifier = new();

    [TestMethod]
    [DataRow(421, SendFailureKind.Transient)]
    [DataRow(450, SendFailureKind.Transient)]
    [DataRow(432, SendFailureKind.SenderAccountPermanent)]
    [DataRow(530, SendFailureKind.SenderAccountPermanent)]
    [DataRow(535, SendFailureKind.SenderAccountPermanent)]
    [DataRow(538, SendFailureKind.SenderAccountPermanent)]
    public void StatusCode_IsClassified(int statusCode, SendFailureKind expected)
    {
        var exception = new SmtpCommandException(
            SmtpErrorCode.UnexpectedStatusCode,
            (SmtpStatusCode)statusCode,
            "smtp error"
        );

        Assert.AreEqual(expected, _classifier.Classify(exception).FailureKind);
    }

    [TestMethod]
    public void RecipientAndMessageFailures_DoNotInvalidateSenderAccount()
    {
        var recipient = new SmtpCommandException(
            SmtpErrorCode.RecipientNotAccepted,
            (SmtpStatusCode)550,
            "recipient rejected"
        );
        var message = new SmtpCommandException(
            SmtpErrorCode.MessageNotAccepted,
            (SmtpStatusCode)554,
            "message rejected"
        );

        Assert.AreEqual(
            SendFailureKind.RecipientPermanent,
            _classifier.Classify(recipient).FailureKind
        );
        Assert.AreEqual(
            SendFailureKind.MessagePermanent,
            _classifier.Classify(message).FailureKind
        );
    }

    [TestMethod]
    public void RecipientHardBounce_WithRecipientAddress_IsMarkedForRecipientContactCleaning()
    {
        var exception = new SmtpCommandException(
            SmtpErrorCode.RecipientNotAccepted,
            SmtpStatusCode.MailboxUnavailable,
            new MailboxAddress("recipient", "missing@example.com"),
            "recipient rejected"
        );

        var result = _classifier.Classify(exception);

        Assert.AreEqual(SendFailureKind.HardBounce, result.FailureKind);
        Assert.AreEqual("missing@example.com", result.RejectedRecipientEmail);
    }

    [TestMethod]
    public void NonProtocolFailures_AreStable()
    {
        Assert.AreEqual(
            SendFailureKind.Cancelled,
            _classifier.Classify(new OperationCanceledException()).FailureKind
        );
        Assert.AreEqual(
            SendFailureKind.SenderAccountPermanent,
            _classifier.Classify(new AuthenticationException()).FailureKind
        );
        Assert.AreEqual(
            SendFailureKind.Network,
            _classifier.Classify(new SocketException()).FailureKind
        );
        Assert.AreEqual(
            SendFailureKind.Unknown,
            _classifier.Classify(new InvalidOperationException()).FailureKind
        );
    }
}
