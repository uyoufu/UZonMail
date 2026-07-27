using System;
using Microsoft.AspNetCore.Http;
using UzonMail.Utils.Resources.Langs;

namespace UzonMail.Utils.Web.Exceptions
{
    /// <summary>
    /// 已知的异常
    /// 通常在编程中主动抛出，用于控制流程
    /// </summary>
    public class KnownException : Exception
    {
        public int Code { get; set; } = StatusCodes.Status500InternalServerError;

        /// <summary>
        /// 当前异常对应的本地化错误描述。
        /// </summary>
        public LocalizedApiError? LocalizedError { get; }

        public KnownException(string message)
            : base(message) { }

        /// <summary>
        /// 使用稳定错误键创建面向 HTTP 客户端的业务异常。
        /// </summary>
        public KnownException(LocalizedApiError localizedError)
            : base(localizedError.Key.ToString())
        {
            LocalizedError = localizedError;
        }
    }
}
