using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Emails
{
    public class SmtpInfoService(SqlContext db) : IScopedService
    {
        /// <summary>
        /// 更新 Smtp 信息
        /// </summary>
        /// <param name="smtpInfo"></param>
        /// <returns></returns>
        public async Task<SmtpInfo> UpdateSmtpInfo(SmtpInfo smtpInfo)
        {
            // domain 只取 @ 后面部分
            smtpInfo.Domain = MailServerInfoGuess.Domain(smtpInfo.Domain);
            var existOne = await db.SmtpInfos.FirstOrDefaultAsync(x => x.Domain == smtpInfo.Domain);
            if (existOne == null)
            {
                db.SmtpInfos.Add(smtpInfo);
                existOne = smtpInfo;
            }
            else
            {
                existOne.Host = smtpInfo.Host;
                existOne.Port = smtpInfo.Port;
                existOne.ConnectionSecurity = smtpInfo.ConnectionSecurity;
                existOne.EnableSSL = smtpInfo.EnableSSL;
            }

            await db.SaveChangesAsync();
            return existOne;
        }

        /// <summary>
        /// 获取 Smtp 信息
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, SmtpInfo>> GuessSmtpInfos(List<string> emails)
        {
            // 验证
            var validEmails = MailServerInfoGuess.ValidEmails(emails);
            if (validEmails.Count == 0)
            {
                return [];
            }

            var validDomains = MailServerInfoGuess.Domains(validEmails);
            var smtpInfos = await db
                .SmtpInfos.AsNoTracking()
                .Where(x => validDomains.Contains(x.Domain))
                .ToListAsync();

            return MailServerInfoGuess.BuildResults(
                validEmails,
                smtpInfos,
                domain => new SmtpInfo
                {
                    Domain = domain,
                    Host = "smtp." + domain,
                    Port = 465,
                    ConnectionSecurity = ConnectionSecurity.SSL,
                    EnableSSL = true
                }
            );
        }
    }
}
