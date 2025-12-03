using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.Core.Utils.FluentMailkit;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Notification.EmailNotifier
{
    /// <summary>
    /// 发件组完成通知
    /// </summary>
    /// <param name="systemSetting"></param>
    /// <param name="userSetting"></param>
    public class SendingGroupFinishedNotification(
        SqlContext db,
        AppSettingService systemSetting,
        UserSettingService userSetting,
        AppSettingsManager settingsManager
    ) : IScopedService
    {
        public static readonly string NotificationTemplateKey =
            "sendingGroupFinishedNotificationTemplate";

        public async Task Notify(SendingGroup sendingGroup)
        {
            // 发送项目组完成通知
            var settings = await settingsManager.GetSetting<SmtpNotificationSetting>(
                db,
                sendingGroup.UserId
            );
            if (!settings.IsValid)
                return;

            // 获取用户的接收邮箱
            var email = await userSetting.GetNotificationRecipientEmail(sendingGroup.UserId);
            if (string.IsNullOrEmpty(email))
                return;

            if (!email.Contains('@'))
                return;

            // 获取发件内容
            var template = await db.AppSettings.FirstOrDefaultAsync(x =>
                x.Key == NotificationTemplateKey
            );
            if (template == null || string.IsNullOrEmpty(template.StringValue))
            {
                return;
            }

            // 更新模板数据
            var spentDate = sendingGroup.SendEndDate - sendingGroup.SendStartDate;
            var htmlBody = template
                .StringValue.Replace("{{id}}", sendingGroup.Id.ToString())
                .Replace("{{type}}", sendingGroup.SendingType.ToString())
                .Replace("{{subject}}", sendingGroup.Subjects)
                .Replace("{{outboxCount}}", sendingGroup.OutboxesCount.ToString())
                .Replace("{{inboxCount}}", sendingGroup.InboxesCount.ToString())
                .Replace("{{successCount}}", sendingGroup.SuccessCount.ToString())
                .Replace(
                    "{{startDate}}",
                    sendingGroup.SendStartDate.ToString("yyyy-MM-dd HH:mm:ss")
                )
                .Replace(
                    "{{spentHours}}",
                    $"{spentDate.Hours} 时 {spentDate.Minutes} 分 {spentDate} 秒"
                );

            await new FluentEmail()
                .WithFrom(settings.Email, "UZonMail")
                .WithTo(email)
                .WithSubject($"【SMTP 通知】您的发件任务 #{sendingGroup.Id} 已完成")
                .WithHtmlBody(htmlBody)
                .ConnectTo(settings.SmtpHost, settings.SmtpPort, true)
                .Authenticate(settings.Email, settings.Password)
                .SendAsync();
        }
    }
}
