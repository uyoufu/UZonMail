using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

public readonly record struct SmtpClientKey(
    SenderAccountKey SenderAccount,
    string ProfileFingerprint,
    string RouteIdentity,
    string Email
)
{
    public bool HasProxy => RouteIdentity != "direct";
}
