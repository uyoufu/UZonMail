using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Json;

namespace UzonMail.CorePlugin.Database.Initializers;

/// <summary>
/// 从现有 SMTP 域名生成 IMAP 默认值，并用服务商特例覆盖通用规则。
/// </summary>
public sealed class InitImapInfo(SqlContext db) : IDbInitializer
{
    public string Name => nameof(InitImapInfo);

    public async Task ExecuteAsync()
    {
        var assemblyDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("无法确定程序集目录");
        var smtpInfoPath = Path.Combine(assemblyDirectory, "data/init/smtpInfo.json");
        if (!File.Exists(smtpInfoPath))
            return;

        var smtpInfos = (await File.ReadAllTextAsync(smtpInfoPath)).JsonTo<List<SmtpInfo>>();
        if (smtpInfos == null || smtpInfos.Count == 0)
            return;

        var overridePath = Path.Combine(assemblyDirectory, "data/init/imapInfoOverrides.json");
        var overrides = File.Exists(overridePath)
            ? (await File.ReadAllTextAsync(overridePath)).JsonTo<List<ImapInfo>>() ?? []
            : [];
        var overridesByDomain = overrides.ToDictionary(info =>
            info.Domain.Trim().ToLowerInvariant()
        );
        var existingDomains = await db.ImapInfos.Select(info => info.Domain).ToListAsync();
        var existingDomainSet = existingDomains.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var imapInfos = smtpInfos
            .Select(CreateImapInfo)
            .Where(info => !existingDomainSet.Contains(info.Domain))
            .ToList();
        await db.ImapInfos.AddRangeAsync(imapInfos);
        await db.SaveChangesAsync();

        ImapInfo CreateImapInfo(SmtpInfo smtpInfo)
        {
            var domain = smtpInfo.Domain.Trim().ToLowerInvariant();
            if (overridesByDomain.TryGetValue(domain, out var providerOverride))
            {
                providerOverride.Domain = domain;
                return providerOverride;
            }

            return new ImapInfo
            {
                Domain = domain,
                Host = $"imap.{domain}",
                Port = 993,
                ConnectionSecurity = ConnectionSecurity.SSL,
            };
        }
    }
}
