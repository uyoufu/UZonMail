using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Validators;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Emails
{
    public class SmtpInfoService(SqlContext db) : IScopedService
    {
        /// <summary>
        /// 获取 Smtp 信息
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, SmtpInfo>> GuessSmtpInfos(List<string> emails)
        {
            // 验证
            var validEmails = emails.Where(email => email.IsValidEmail()).Distinct().ToList();
            if (validEmails.Count == 0)
            {
                return [];
            }

            var validDomains = validEmails.Select(email => email.Split('@')[1]).Distinct().ToList();
            var smtpInfos = await db.SmtpInfos.AsNoTracking().Where(x => validDomains.Contains(x.Domain)).ToListAsync();

            // 返回结果，若数据库中不存在，则使用默认的 Smtp 信息
            var results = new Dictionary<string, SmtpInfo>();
            foreach (var email in validEmails)
            {
                var domain = email.Split('@')[1];
                var smtpInfo = smtpInfos.Find(x => x.Domain == domain);
                smtpInfo ??= new SmtpInfo()
                    {
                        Domain = domain,
                        Host = "smtp." + domain,
                        Port = 465,
                        SecurityProtocol = SecurityProtocol.SSL,
                        EnableSSL = true
                    };
                smtpInfos.Add(smtpInfo);

                results.Add(email,smtpInfo);
            }

            return results;
        }
    }
}
