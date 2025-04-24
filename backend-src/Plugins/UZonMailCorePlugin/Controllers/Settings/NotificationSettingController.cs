using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Settings.Models;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.Emails;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    /// <summary>
    /// 通知设置
    /// </summary>
    /// <param name="db"></param>
    public class NotificationSettingController(SqlContext db) : ControllerBaseV1
    {
        /// <summary>
        /// 验证通知邮箱设置
        /// </summary>
        /// <returns></returns>
        [HttpPut("validate")]
        public async Task<ResponseResult<bool>> ValidateNotificationEmailSettings()
        {
            string settingKey = "systemSmtpNotification";
            var settings = await db.SystemSettings.Where(x => x.Key == settingKey)
            .FirstOrDefaultAsync();

            var smtpSettings = settings!.Json!.ToObject<SystemSmtpNotificationSetting>();
            if (smtpSettings == null)
            {
                return false.ToFailResponse("未找到通知邮箱设置");
            }

            // 开始验证
            var outboxTestor = new OutboxTestSender(db);
            var result = outboxTestor.SendTest(smtpSettings.SmtpHost, smtpSettings.SmtpPort, true, smtpSettings.Email, smtpSettings.Password);

            if (!result.Ok)
                return false.ToFailResponse(result.Message);

            // 验证通过后，更新数据库
            smtpSettings.IsValid = true;

            var serializer = JsonSerializer.CreateDefault(JsonExtension.CameCaseJsonSettings);
            settings.Json = JObject.FromObject(smtpSettings, serializer);
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
