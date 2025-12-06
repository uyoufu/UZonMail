using System.Reflection;
using UZonMail.CorePlugin.Database.Updater;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Json;

namespace UZonMail.CorePlugin.Database.Update.Updaters
{
    public class InitSmtpInfo(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.12.4");

        public async Task Update()
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

            await db.SmtpInfos.AddRangeAsync(smtpInfos);
            await db.SaveChangesAsync();
        }
    }
}
