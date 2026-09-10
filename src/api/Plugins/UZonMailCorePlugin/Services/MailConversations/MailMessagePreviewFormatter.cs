using System.Net;
using System.Text.RegularExpressions;

namespace UzonMail.CorePlugin.Services.MailConversations;

/// <summary>
/// 将远端摘要和外发 HTML 收敛为可安全用于列表的短文本。
/// </summary>
public static partial class MailMessagePreviewFormatter
{
    public const int MaximumLength = 500;

    public static string? Normalize(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var withoutTags = HtmlTagPattern().Replace(content, " ");
        var normalized = WhitespacePattern()
            .Replace(WebUtility.HtmlDecode(withoutTags), " ")
            .Trim();
        if (normalized.Length == 0)
            return null;

        return normalized.Length <= MaximumLength ? normalized : normalized[..MaximumLength];
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespacePattern();
}
