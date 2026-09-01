using MailKit.Net.Imap;

namespace UzonMail.CorePlugin.Services.EmailReceiving;

/// <summary>
/// 向要求 RFC 2971 ID 的 IMAP 服务商声明客户端身份。
/// </summary>
internal static class ImapClientIdentification
{
    private const string ClientName = "UzonMail";
    private const string ClientVendor = "UzonMail";
    private const string ClientSupportUrl = "https://github.com/uyoufu/UzonMail";
    private const string ClientSupportEmail = "uyoufu@uzoncloud.com";
    private const string SupportEmailProperty = "support-email";

    /// <summary>
    /// 在认证后发送客户端身份；网易系邮箱会在缺少该命令时拒绝打开邮箱。
    /// </summary>
    public static async Task IdentifyAsync(ImapClient client, CancellationToken cancellationToken)
    {
        if (!SupportsIdentification(client.Capabilities))
            return;

        await client.IdentifyAsync(CreateClientImplementation(), cancellationToken);
    }

    internal static bool SupportsIdentification(ImapCapabilities capabilities) =>
        capabilities.HasFlag(ImapCapabilities.Id);

    internal static ImapImplementation CreateClientImplementation()
    {
        var implementation = new ImapImplementation
        {
            Name = ClientName,
            Version = typeof(ImapClientIdentification).Assembly.GetName().Version?.ToString(),
            Vendor = ClientVendor,
            SupportUrl = ClientSupportUrl,
        };
        implementation.Properties[SupportEmailProperty] = ClientSupportEmail;
        return implementation;
    }
}
