using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.Core.Utils.Extensions;
using UZonMail.Core.Database.Validators;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.DB.SQL.Core.Templates;
using UZonMail.DB.Utils;


namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// 邮箱模板
    /// </summary>
    public class EmailTemplateController(SqlContext db, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 通过 id 或者名称获取邮件模板
        /// 只会返回成功的结果
        /// 根据是否为空来判断是否成功
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        [HttpGet("by-id-or-name")]
        public async Task<ResponseResult<EmailTemplate?>> GetEmailTemplateByIdOrName(long templateId, string templateName)
        {
            var userId = tokenService.GetUserSqlId();
            var result = await db.EmailTemplates.FirstOrDefaultAsync(x => x.UserId == userId && (x.Id == templateId || x.Name == templateName));
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 通过 id 获取邮件模板
        /// </summary>
        /// <param name="emailTemplateId"></param>
        /// <returns></returns>
        [HttpGet("{emailTemplateId:long}")]
        public async Task<ResponseResult<EmailTemplate?>> GetEmailTemplateById(long emailTemplateId)
        {
            var userId = tokenService.GetUserSqlId();
            var result = await db.EmailTemplates.FirstOrDefaultAsync(x => x.Id == emailTemplateId && x.UserId == userId);
            return result.ToSmartResponse();
        }

        /// <summary>
        /// 新增或修改邮件模板
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<EmailTemplate>> Upsert([FromBody] EmailTemplate entity)
        {
            // 添加当前用户名
            entity.UserId = tokenService.GetUserSqlId();

            // 数据验证
            var emailTemplateValidator = new EmailTemplateValidator();
            var vdResult = emailTemplateValidator.Validate(entity);
            if (!vdResult.IsValid)
            {
               return vdResult.ToErrorResponse<EmailTemplate>();
            }

            // 判断是否存在同名的模板
            var existOne = await db.EmailTemplates.FirstOrDefaultAsync(x => x.Id == entity.Id && x.UserId == entity.UserId);
            // 如果有 Id,则说明是修改
            if (entity.Id > 0)
            {
                if (existOne != null && existOne.Id != entity.Id)
                    throw new KnownException("已存在同名的模板");

                if (existOne == null)
                    throw new KnownException("未找到模板");

                existOne.Name = entity.Name;
                existOne.Content = entity.Content;
                // 更新缩略图
                if (!string.IsNullOrEmpty(existOne.Thumbnail))
                {
                    if (existOne.Thumbnail.Contains('?'))
                    {
                        // 包含?号，将后面的值替换
                        existOne.Thumbnail = existOne.Thumbnail.Substring(0, existOne.Thumbnail.IndexOf('?')) + $"?v={DateTime.Now.ToTimestamp()}";
                    }
                    else
                    {
                        existOne.Thumbnail = existOne.Thumbnail + $"?v={DateTime.Now}";
                    }
                }

                await db.UpdateById(entity, [nameof(EmailTemplate.Name), nameof(EmailTemplate.Content)]);
            }
            else
            {
                if (existOne != null)
                    throw new KnownException("已存在同名的模板");

                db.Add(entity);
                await db.SaveChangesAsync();
                // 更新缩略图
                entity.Thumbnail = $"public/{entity.UserId}/template-thumbnails/{entity.Id}.png";
                await db.SaveChangesAsync();
            }

            return entity.ToSuccessResponse();
        }

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public async Task<ResponseResult<bool>> Delete(long id)
        {
            // 通过条件删除
            var userId = tokenService.GetUserSqlId();
            var email = await db.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId) ?? throw new KnownException("模板不存在");
            db.Remove(email);
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮件模板数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetEmailTemplatesCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db.EmailTemplates.Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Content.Contains(filter));
            }
            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮件模板数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<EmailTemplate>>> GetEmailTemplatesData(string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db.EmailTemplates.Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Content.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }
    }
}
