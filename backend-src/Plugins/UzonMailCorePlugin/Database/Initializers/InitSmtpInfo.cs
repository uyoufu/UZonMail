using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Json;

namespace UZonMail.CorePlugin.Database.Initializers
{
    public class InitSmtpInfo(SqlContext db) : IDbInitializer
    {
        public string Name => nameof(InitSmtpInfo);

        public async Task ExecuteAsync()
        {
            // 获取当前程序集所在目录
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            var smtpInfoPath = Path.Combine(assemblyDirectory, "data/init/smtpInfo.json");
            if (!File.Exists(smtpInfoPath))
            {
                // 如果文件不存在，直接返回
                return;
            }

            var smtpInfoJson = await File.ReadAllTextAsync(smtpInfoPath);
            var smtpInfos = smtpInfoJson.JsonTo<List<SmtpInfo>>();
            if (smtpInfos == null || smtpInfos.Count == 0)
            {
                // 如果没有数据，直接返回
                return;
            }

            // 进行去重
            var existDomains = await db.SmtpInfos.Select(x => x.Domain.ToLower()).ToListAsync();
            var newSmtpInfos = smtpInfos
                .Where(x => !existDomains.Contains(x.Domain.ToLower()))
                .ToList();

            await db.SmtpInfos.AddRangeAsync(newSmtpInfos);
            await db.SaveChangesAsync();
        }
    }
}
