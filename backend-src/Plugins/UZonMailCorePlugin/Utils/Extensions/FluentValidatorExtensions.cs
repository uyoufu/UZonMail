using FluentValidation.Results;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Utils.Extensions
{
    public static class FluentValidatorExtensions
    {
        /// <summary>
        /// 转换为错误响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static ErrorResponse<T> ToErrorResponse<T>(this ValidationResult result)
        {
            var errorMessage = string.Join(";", result.Errors);
            return new ErrorResponse<T>(errorMessage);
        }
    }
}
