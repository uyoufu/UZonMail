using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Settings
{
    public class UserSettingService(SqlContext db) : IScopedService
    {
        private static readonly string _notificationRecipientEmailKey =
            "notificationRecipientEmail";

        /// <summary>
        /// 获取用户的通知接收邮箱
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string?> GetNotificationRecipientEmail(long userId)
        {
            var email = await db
                .AppSettings.Where(x => x.Type == AppSettingType.User && x.UserId == userId)
                .Where(x => x.Key == _notificationRecipientEmailKey)
                .Select(x => x.StringValue)
                .FirstOrDefaultAsync();

            return email;
        }
    }
}
