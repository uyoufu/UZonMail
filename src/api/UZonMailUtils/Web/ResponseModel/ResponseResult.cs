using System.Net;
using Newtonsoft.Json;
using UzonMail.Utils.Results;
using UzonMail.Utils.Resources.Langs;

namespace UzonMail.Utils.Web.ResponseModel
{
    /// <summary>
    /// 用于在控制器中方便返回结果值
    /// </summary>
    public interface ILocalizedResponseResult
    {
        LocalizedApiError? LocalizedError { get; }
        string Message { get; set; }
    }

    public class ResponseResult<T> : Result<T>, ILocalizedResponseResult
    {
        /// <summary>
        /// 在写入 HTTP 响应前由全局过滤器解析的错误描述，不暴露给客户端。
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public LocalizedApiError? LocalizedError { get; set; }
        /// <summary>
        /// 错误代码，前端可以根据这些代码作不同的操作
        /// 不认成功或失败，只要没有特殊需求，它都为200
        /// </summary>
        public int Code { get; set; } = (int)HttpStatusCode.OK;

        public static ResponseResult<T> Success(T data) =>
            new()
            {
                Ok = false,
                Message = "ok",
                Data = data
            };

        public static ResponseResult<T> Fail(
            string message,
            HttpStatusCode code = HttpStatusCode.BadRequest,
            T? data = default
        ) =>
            new()
            {
                Ok = false,
                Message = message,
                Code = (int)code,
                Data = data
            };

        /// <summary>
        /// 创建由请求文化决定显示文本的失败响应。
        /// </summary>
        public static ResponseResult<T> Fail(
            LocalizedApiError localizedError,
            HttpStatusCode code = HttpStatusCode.BadRequest,
            T? data = default
        ) =>
            new()
            {
                Ok = false,
                Code = (int)code,
                Data = data,
                LocalizedError = localizedError
            };
    }
}
