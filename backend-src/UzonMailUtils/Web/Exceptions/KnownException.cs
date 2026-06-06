using Microsoft.AspNetCore.Http;
using System;

namespace UZonMail.Utils.Web.Exceptions
{
    /// <summary>
    /// 已知的异常
    /// 通常在编程中主动抛出，用于控制流程
    /// </summary>
    public class KnownException : Exception
    {
        public int Code { get; set; } = StatusCodes.Status500InternalServerError;

        public KnownException(string message) : base(message)
        {
        }
    }
}
