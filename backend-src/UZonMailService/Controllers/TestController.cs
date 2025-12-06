using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailService.Resources.Langs;

namespace UZonMail.Server.Controllers
{
    /// <summary>
    /// 测试用的控制器
    /// </summary>
    [Authorize]
    [Route("api/v2/[controller]")]
    [ApiController]
    public class TestController(IStringLocalizer<SharedResource> localizer2)
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
            return localizer2.GetString(name).Value.ToSuccessResponse();
        }
    }
}
