using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Resources.Langs;
using UZonMail.CorePlugin.SignalRHubs;
using UZonMail.CorePlugin.SignalRHubs.Extensions;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.Tests
{
    /// <summary>
    /// 测试用的控制器
    /// </summary>
    public class TestController(
        IStringLocalizer<SharedResource> localizer,
        IHubContext<UzonMailHub, IUzonMailClient> hubClient
    ) : ControllerBaseV1
    {
        /// <summary>
        /// Retrieves a localized string resource for the specified name.
        /// </summary>
        /// <param name="name">The key name of the localized string to retrieve. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response with the localized
        /// string value.</returns>
        [HttpGet("i18n")]
        public async Task<ResponseResult<string>> GetI18N([FromQuery] string name)
        {
            return localizer.GetString(name).Value.ToSuccessResponse();
        }

        [HttpGet("hub-client")]
        public async Task<ResponseResult<bool>> TestSignalR([FromQuery] string name)
        {
            var client = hubClient.GetUserClient(100);
            // 即使不存在，也不会返回空
            if (client == null)
            {
                return false.ToSuccessResponse();
            }
            return true.ToSuccessResponse();
        }
    }
}
