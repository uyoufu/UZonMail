using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Emails;

/// <summary>
/// 按邮箱域名提供 IMAP 连接参数，未知域使用标准安全端口作为回退。
/// </summary>
public sealed class ImapInfoService(SqlContext db) : IScopedService
{
    public async Task<Dictionary<string, ImapInfo>> GuessImapInfos(List<string> emails)
    {
        var validEmails = MailServerInfoGuess.ValidEmails(emails);
        if (validEmails.Count == 0)
            return [];

        var validDomains = MailServerInfoGuess.Domains(validEmails);
        var imapInfos = await db
            .ImapInfos.AsNoTracking()
            .Where(info => validDomains.Contains(info.Domain))
            .ToListAsync();

        return MailServerInfoGuess.BuildResults(
            validEmails,
            imapInfos,
            domain => new ImapInfo
            {
                Domain = domain,
                Host = $"imap.{domain}",
                Port = 993,
                ConnectionSecurity = ConnectionSecurity.SSL,
            }
        );
    }
}
