using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Token;

namespace UZonMail.CorePlugin.Controllers
{
    /// <summary>
    /// 所有控制器基接口
    /// </summary>
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public abstract class ControllerBaseV1 : ControllerBase
    {
        #region 控制器通用的方法
        private JObject? _requestBody;

        /// <summary>
        /// 请求数据体
        /// </summary>
        protected JObject RequestBody
        {
            get
            {
                if (_requestBody == null)
                {
                    StreamReader sr = new(Request.Body);
                    var json = sr.ReadToEndAsync().GetAwaiter().GetResult();
                    // 读取数据
                    _requestBody = JObject.Parse(json);
                }

                return _requestBody;
            }
        }

        /// <summary>
        /// 从 token 中获取 userId
        /// </summary>
        /// <param name="tokenParams"></param>
        /// <returns></returns>
        protected int GetUserIdFromToken(TokenParams tokenParams)
        {
            var token = Request.Headers[HeaderNames.Authorization].ToString();
            if (string.IsNullOrEmpty(token))
                return 0;

            var tokenPayloads = JWTToken.GetTokenPayloads(token);
            string userId = tokenPayloads.SelectTokenOrDefault("userId", string.Empty);
            if (int.TryParse(userId, out int intUserId))
                return intUserId;
            return 0;
        }
        #endregion
    }
}
