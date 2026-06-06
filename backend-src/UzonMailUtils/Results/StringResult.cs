using System;
using System.Collections.Generic;
using System.Text;

namespace UZonMail.Utils.Results
{
    /// <summary>
    /// 字符串结果
    /// </summary>
    public class StringResult : Result<string>
    {
        public StringResult(bool ok, string message, string data = "") : base(ok, message, data)
        {
        }

        /// <summary>
        /// 返回一个成功的结果
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static StringResult Success(string data) => new StringResult(true, "success", data);

        /// <summary>
        /// 返回一个失败的结果
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static StringResult Fail(string message) => new StringResult(false, message);
    }
}
