using Microsoft.AspNetCore.Mvc;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.DB.SQL.Base;
using UZonMail.Core.Services.Common;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Common
{
    /// <summary>
    /// 通用的增删改查控制器
    /// </summary>
    public abstract class CurdController<T>(CurdService<T> curdService) : ControllerBaseV1 where T : SqlId
    {
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<ResponseResult<T>> Create([FromBody] T entity)
        {
            // 需要判断是否重复
            var result = await curdService.Create(entity);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 更新
        /// 若要起作用，则子类需要重新实现该方法
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("{id:long}")]
        public abstract Task<ResponseResult<T>> Update(long id, [FromBody] T entity);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public virtual async Task<ResponseResult<bool>> Delete(long id)
        {
            var result = await curdService.DeleteById(id);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:long}")]
        public virtual async Task<ResponseResult<T?>> FindOneById(long id)
        {
            var result = await curdService.FindOneById(id);
            return result.ToSuccessResponse();
        }
    }
}
