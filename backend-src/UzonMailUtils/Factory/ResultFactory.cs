using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Results;

namespace UZonMail.Utils.Factory
{
    /// <summary>
    /// 此静态类用于更方便地生成结果
    /// 为什么不在定义中添加静态
    /// 因为定义中是泛型，在使用时IDE无法推断类型
    /// 在extension内部不使用该类，避免偶合过多
    /// </summary>
    public class ResultFactory
    {
        /// <summary>
        /// 返回成功结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<T> SuccessResult<T>(T data,string message="success")
        {
            return new Result<T>()
            {
                Ok = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// 错误结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<T> ErrorResult<T>(string message)
        {
            return new Result<T>()
            {
                Ok = false,
                Message = message
            };
        }

        /// <summary>
        /// 成功响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResponseResult<T> SuccessResponse<T>(T data, string message = "success")
        {
            return new ResponseResult<T>()
            {
                Ok = true,
                Data = data,
                Message = message
            };
        }


        /// <summary>
        /// 失败响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResponseResult<T> ErrorResponse<T>(string message)
        {
            return new ResponseResult<T>()
            {
                Ok = false,
                Message = message
            };
        }
    }
}
