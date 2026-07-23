using System.Net;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.Clients
{
    public sealed record ProxyEndpoint(
        string Scheme,
        string Host,
        int Port,
        string Username,
        string Password,
        string SourceUrl
    )
    {
        public static readonly ISet<string> SupportedSchemes = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "http",
            "https",
            "socks4",
            "socks4a",
            "socks5",
        };

        public NetworkCredential Credentials => new(Username, Password);

        public bool HasSameConnection(ProxyEndpoint other)
        {
            return string.Equals(Scheme, other.Scheme, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase)
                && Port == other.Port
                && Username == other.Username
                && Password == other.Password;
        }

        public static bool CanParse(string proxyUrl)
        {
            return TryCreate(proxyUrl, out _, out _);
        }

        public static bool TryCreate(
            Proxy proxy,
            out ProxyEndpoint? endpoint,
            out string errorMessage
        )
        {
            return TryCreate(proxy.Url, out endpoint, out errorMessage);
        }

        public static bool TryCreate(
            string proxyUrl,
            out ProxyEndpoint? endpoint,
            out string errorMessage
        )
        {
            endpoint = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                errorMessage = "代理地址不能为空";
                return false;
            }

            if (!Uri.TryCreate(proxyUrl, UriKind.Absolute, out var uri))
            {
                errorMessage = "代理地址格式不正确";
                return false;
            }

            if (!SupportedSchemes.Contains(uri.Scheme))
            {
                errorMessage = $"不支持的代理协议: {uri.Scheme}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri.Host) || uri.Port <= 0)
            {
                errorMessage = "代理地址必须包含 host 和 port";
                return false;
            }

            if (
                !string.IsNullOrEmpty(uri.Query)
                || (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
            )
            {
                errorMessage = "单代理地址不能包含 path 或 query";
                return false;
            }

            var username = string.Empty;
            var password = string.Empty;
            var userInfo = uri.UserInfo;
            if (!string.IsNullOrEmpty(userInfo))
            {
                var parts = userInfo.Split(':', 2);
                username = Uri.UnescapeDataString(parts[0]);
                if (parts.Length > 1)
                    password = Uri.UnescapeDataString(parts[1]);
            }

            endpoint = new ProxyEndpoint(
                uri.Scheme.ToLowerInvariant(),
                uri.Host,
                uri.Port,
                username,
                password,
                proxyUrl
            );
            return true;
        }
    }
}
