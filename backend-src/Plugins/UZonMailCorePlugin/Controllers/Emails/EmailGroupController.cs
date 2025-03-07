using Microsoft.AspNetCore.Mvc;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Common;
using UZonMail.Core.Services.Emails;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL.Emails;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Core.Services.Common;
using UZonMail.DB.SQL;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// 邮件管理
    /// </summary>
    public class EmailGroupController(SqlContext db, EmailGroupService groupService, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet("{groupId:long}")]
        public async Task<ResponseResult<EmailGroup?>> FindOneById(long groupId)
        {
            var result = await db.EmailGroups.Where(x => x.Id == groupId).FirstOrDefaultAsync();
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 创建邮件组
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<EmailGroup>> Create([FromBody] EmailGroup entity)
        {
            var userId = tokenService.GetUserSqlId();
            entity.UserId = userId;

            EmailGroup emailGroup = await groupService.Create(entity);
            return emailGroup.ToSuccessResponse();
        }

        /// <summary>
        /// 获取用户组
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<ResponseResult<List<EmailGroup>>> GetEmailGroups([FromQuery] EmailGroupType type)
        {
            var userId = tokenService.GetUserSqlId();
            var groups = await groupService.GetEmailGroups(userId, type);
            return groups.ToSuccessResponse();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("{id:long}")]
        public async Task<ResponseResult<EmailGroup>> Update(long id, [FromBody] EmailGroup entity)
        {
            // 数据验证
            if (string.IsNullOrEmpty(entity.Name)) throw new KnownException("组名不允许为空");

            entity.Id = id;
            await groupService.Update(entity, [nameof(EmailGroup.Name), nameof(EmailGroup.Description), nameof(EmailGroup.Order)]);
            return entity.ToSuccessResponse();
        }

        /// <summary>
        /// 根据 id 删除组
        /// 组可能已经被使用，若被使用，则不允许删除
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpDelete("{groupId:long}")]
        public async Task<ResponseResult<bool>> Delete(long groupId)
        {
            var userId = tokenService.GetUserSqlId();
            // 将组重新命名
            var emailGroup = await db.EmailGroups.Where(x => x.Id == groupId && x.UserId == userId).FirstOrDefaultAsync();
            if (emailGroup == null) return false.ToFailResponse("未找到该邮件组");

            // 将组进行重命名
            emailGroup.Name += "_deletedAt" + DateTime.Now.ToString("D");
            emailGroup.IsDeleted = true;
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 删除组中所有无效的发件箱
        /// </summary>
        /// <param name="outboxIds"></param>
        /// <returns></returns>
        [HttpDelete("{groupId:long}/invalid-outboxes")]
        public async Task<ResponseResult<bool>> DeleteAllInvalidBoxesInGroup(long groupId)
        {
            // 判断是否属于自己的组
            var userId = tokenService.GetUserSqlId();
            var emailBoxes = db.Outboxes.Where(x => !x.IsValid && x.EmailGroupId == groupId && x.UserId == userId);
            db.Outboxes.RemoveRange(emailBoxes);
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
