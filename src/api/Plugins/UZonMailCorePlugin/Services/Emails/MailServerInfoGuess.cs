using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Validators;

namespace UzonMail.CorePlugin.Services.Emails;

/// <summary>
/// 统一 SMTP 与 IMAP 的邮箱校验、域名规范化和默认结果组装规则。
/// </summary>
internal static class MailServerInfoGuess
{
    internal static List<string> ValidEmails(IEnumerable<string> emails) =>
        emails.Where(email => email.IsValidEmail()).Distinct().ToList();

    internal static List<string> Domains(IEnumerable<string> emails) =>
        emails.Select(Domain).Distinct().ToList();

    internal static string Domain(string emailOrDomain) =>
        emailOrDomain.Split('@').Last().Trim().ToLowerInvariant();

    internal static Dictionary<string, TServerInfo> BuildResults<TServerInfo>(
        IEnumerable<string> emails,
        IEnumerable<TServerInfo> knownServerInfos,
        Func<string, TServerInfo> createFallback
    )
        where TServerInfo : IMailServerInfo
    {
        var knownByDomain = knownServerInfos
            .GroupBy(info => Domain(info.Domain))
            .ToDictionary(group => group.Key, group => group.First());

        return emails.ToDictionary(
            email => email,
            email =>
            {
                var domain = Domain(email);
                return knownByDomain.GetValueOrDefault(domain) ?? createFallback(domain);
            }
        );
    }
}
