using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.ServerSystem
{
    public class SystemInfoController : ControllerBaseV1
    {
        /// <summary>
        /// 获取系统版本
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("version")]
        public ResponseResult<string> GetSystemVersion()
        {
            // 获取当前程序集的版本           
            var version = Assembly.GetEntryAssembly().GetName().Version;
            return version.ToString().ToSuccessResponse();
        }
    }
}
