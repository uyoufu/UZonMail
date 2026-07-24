using MailKit.Security;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

public sealed record SmtpConnectionProfile(
    string Host,
    int Port,
    SecureSocketOptions Security,
    string UserName,
    string Password,
    bool SkipAuthentication = false
);

/// <summary>
/// SMTP 发送与发件箱验证共用的连接、TLS 和认证流程。
/// </summary>
public sealed class SmtpConnector : ISingletonService
{
    public async Task ConnectAndAuthenticateAsync(
        ThrottlingSmtpClient client,
        SmtpConnectionProfile profile,
        CancellationToken cancellationToken = default
    )
    {
        await client.ConnectAsync(profile.Host, profile.Port, profile.Security, cancellationToken);

        if (!profile.SkipAuthentication)
        {
            await client.AuthenticateAsync(profile.UserName, profile.Password, cancellationToken);
        }
    }
}
