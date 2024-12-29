using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Settings.Request;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    public class SystemSettingController(SqlContext db) : ControllerBaseV1
    {
        [HttpPut("base-api-url")]
        public async Task<ResponseResult<bool>> UpdateBaseApiUrl([FromBody] UpdateBaseApiUrlBody dataParams)
        {
            var baseApiUrl = dataParams.BaseApiUrl;
            if (string.IsNullOrEmpty(baseApiUrl)) return false.ToFailResponse("baseUrl不能为空");

            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == SystemSetting.BaseApiUrl);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = SystemSetting.BaseApiUrl,
                    StringValue = baseApiUrl
                };
                db.Add(setting);
            }
            else
            {
                setting.StringValue = baseApiUrl;
            }
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
