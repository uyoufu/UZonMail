using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Database.Updater;
using UZonMail.Core.Services.Notification.EmailNotifier;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Database.Update.Updaters
{
    public class AddSendingFinishedNotificationTemplate(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.13.0.1");

        public async Task Update()
        {
            // 获取当前程序集所在目录
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            var templatePath = Path.Combine(
                assemblyDirectory!,
                "data/init/sendingFinishNotificationTemplate.html"
            );
            if (!File.Exists(templatePath))
            {
                // 如果文件不存在，直接返回
                return;
            }

            var template = await File.ReadAllTextAsync(templatePath);

            var systemSetting = await db.AppSettings.FirstOrDefaultAsync(x =>
                x.Key == SendingGroupFinishedNotification.NotificationTemplateKey
            );
            if (systemSetting == null)
            {
                db.AppSettings.Add(
                    new AppSetting()
                    {
                        Key = SendingGroupFinishedNotification.NotificationTemplateKey,
                        StringValue = template,
                    }
                );
            }
            else
            {
                systemSetting.StringValue = template;
            }

            await db.SaveChangesAsync();
        }
    }
}
