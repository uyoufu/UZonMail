using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Settings
{
    public class UserSettingService(SqlContext db) : IScopedService
    {
        private static readonly string _notificationRecipientEmailKey = "notificationRecipientEmail";

        /// <summary>
        /// 获取用户的通知接收邮箱
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string?> GetNotificationRecipientEmail(long userId)
        {
            var email = await db.SystemSettings
                .Where(x => x.Type == SystemSettingType.User && x.UserId == userId)
                .Where(x => x.Key == _notificationRecipientEmailKey)
                .Select(x => x.StringValue)
                .FirstOrDefaultAsync();

            return email;
        }
    }
}
