using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Core.Database.Validators;
using UZonMail.Core.Services.Emails;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.UserInfos;
using UZonMail.Core.Utils.Database;
using UZonMail.Core.Utils.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Emails;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.SendCore.Sender;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// 邮箱
    /// </summary>
    public class EmailBoxController(SqlContext db, TokenService tokenService, UserService userService, EmailGroupService emailGroupService) : ControllerBaseV1
    {
        /// <summary>
        /// 创建发件箱
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("outbox")]
        public async Task<ResponseResult<Outbox>> CreateOutbox([FromBody] Outbox entity)
        {
            var outboxValidator = new OutboxValidator();
            var vdResult = outboxValidator.Validate(entity);
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<Outbox>();
            }

            // 设置默认端口
            if (entity.SmtpPort == 0) entity.SmtpPort = 25; // 默认端口

            var userId = tokenService.GetUserDataId();
            // 验证发件箱是否存在，若存在，则复用原来的发件箱
            Outbox? existOne = db.Outboxes.SingleOrDefault(x => x.UserId == userId && x.Email == entity.Email);
            if (existOne != null)
            {
                existOne.EmailGroupId = entity.EmailGroupId;
                existOne.SmtpPort = entity.SmtpPort;
                existOne.Password = entity.Password;
                existOne.UserName = entity.UserName;
                existOne.Description = entity.Description;
                existOne.ProxyId = entity.ProxyId;
                existOne.ReplyToEmails = entity.ReplyToEmails;
                existOne.SetStatusNormal();
            }
            else
            {
                // 新建一个发件箱
                entity.UserId = userId;
                db.Outboxes.Add(entity);
                existOne = entity;
            }
            await db.SaveChangesAsync();

            return existOne.ToSuccessResponse();
        }

        /// <summary>
        /// 批量新增发件箱
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("outboxes")]
        public async Task<ResponseResult<List<Outbox>>> CreateOutboxes([FromBody] List<Outbox> entities)
        {
            if (entities == null)
            {
                return ResponseResult<List<Outbox>>.Fail("未能解析发件箱数据");
            }
            var userId = tokenService.GetUserDataId();
            foreach (var entity in entities)
            {
                // 设置默认端口
                if (entity.SmtpPort == 0) entity.SmtpPort = 25;
                // 设置用户
                entity.UserId = userId;

                // 验证数据
                var outboxValidator = new OutboxValidator();
                var vdResult = outboxValidator.Validate(entity);
                if (!vdResult.IsValid)
                {
                    return vdResult.ToErrorResponse<List<Outbox>>();
                }
            }

            List<string> emails = entities.Select(x => x.Email).ToList();
            List<Outbox> existEmails = await db.Outboxes.Where(x => x.UserId == userId && emails.Contains(x.Email)).ToListAsync();
            List<Outbox?> newEntities = emails.Except(existEmails.Select(x => x.Email))
                .Select(x => entities.Find(e => e.Email == x))
                .ToList();

            // 新建发件箱
            await db.Outboxes.AddRangeAsync(newEntities.Where(x => x != null));

            // 更新现有的发件箱
            foreach (var entity in existEmails)
            {
                var newEntity = entities.Find(x => x.Email == entity.Email);
                if (newEntity != null)
                {
                    entity.EmailGroupId = newEntity.EmailGroupId;
                    entity.SmtpPort = newEntity.SmtpPort;
                    entity.UserName = newEntity.UserName;
                    entity.Password = newEntity.Password;
                    entity.EnableSSL = newEntity.EnableSSL;
                    entity.Description = newEntity.Description;
                    entity.ProxyId = newEntity.ProxyId;
                    entity.Name = newEntity.Name;
                    entity.ReplyToEmails = newEntity.ReplyToEmails;
                    entity.SetStatusNormal();
                }
            }
            await db.SaveChangesAsync();

            // 返回所有的结果
            List<Outbox> results = [.. existEmails, .. newEntities];
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 创建发件箱
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("inbox")]
        public async Task<ResponseResult<Inbox>> CreateInbox([FromBody] Inbox entity)
        {
            var inboxValidator = new InboxValidator();
            var vdResult = inboxValidator.Validate(entity);
            if (!vdResult.IsValid) return vdResult.ToErrorResponse<Inbox>();

            var tokenPayloads = tokenService.GetTokenPayloads();
            var userId = tokenPayloads.UserId;
            entity.UserId = userId;
            entity.OrganizationId = tokenPayloads.OrganizationId;

            // 验证发件箱是否存在，若存在，则复用原来的发件箱
            Inbox? existOne = db.Inboxes.IgnoreQueryFilters().SingleOrDefault(x => x.UserId == userId && x.Email == entity.Email);
            if (existOne != null)
            {
                existOne.EmailGroupId = entity.EmailGroupId;
                existOne.Name = entity.Name;
                existOne.Description = entity.Description;
                existOne.SetStatusNormal();
            }
            else
            {
                // 新建一个发件箱               
                db.Inboxes.Add(entity);
                existOne = entity;
            }
            await db.SaveChangesAsync();

            return existOne.ToSuccessResponse();
        }

        /// <summary>
        /// 添加未分组收件箱
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpPost("inbox/ungrouped")]
        public async Task<ResponseResult<Inbox>> CreateUngroupedInbox([FromBody] Inbox entity)
        {
            // 获取未分组的组
            var defaultGroup = await emailGroupService.GetDefaultEmailGroup(EmailGroupType.InBox);
            entity.EmailGroupId = defaultGroup.Id;

            return await CreateInbox(entity);
        }

        /// <summary>
        /// 批量新增发件箱
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("inboxes")]
        public async Task<ResponseResult<List<Inbox>>> CreateInboxes([FromBody] List<Inbox> entities)
        {
            if (entities == null)
            {
                return ResponseResult<List<Inbox>>.Fail("未能解析收件箱数据");
            }

            var userId = tokenService.GetUserDataId();
            foreach (var entity in entities)
            {
                // 设置用户
                entity.UserId = userId;
                var inboxValidator = new InboxValidator();
                var vdResult = inboxValidator.Validate(entity);
                if (!vdResult.IsValid) return vdResult.ToErrorResponse<List<Inbox>>();
            }

            List<string> emails = entities.Select(x => x.Email).ToList();
            List<Inbox> existEmails = await db.Inboxes.IgnoreQueryFilters().Where(x => x.UserId == userId && emails.Contains(x.Email)).ToListAsync();
            List<Inbox?> newEntities = emails.Except(existEmails.Select(x => x.Email))
                .Select(x => entities.Find(e => e.Email == x))
                .ToList();

            // 新建发件箱
            foreach (var entity in newEntities)
            {
                if (entity != null)
                    db.Inboxes.Add(entity);
            }

            // 更新现有的发件箱
            foreach (var entity in existEmails)
            {
                var newEntity = entities.Find(x => x.Email == entity.Email);
                if (newEntity != null)
                {
                    entity.EmailGroupId = newEntity.EmailGroupId;
                    entity.Name = newEntity.Name;
                    entity.Description = newEntity.Description;
                    entity.SetStatusNormal();
                }
            }
            await db.SaveChangesAsync();

            // 返回所有的结果
            List<Inbox> results = [.. existEmails, .. newEntities];
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 更新发件箱
        /// </summary>
        /// <param name="outboxId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("outbox/{outboxId:long}")]
        public async Task<ResponseResult<bool>> UpdateOutbox(long outboxId, [FromBody] Outbox entity)
        {
            await db.Outboxes.UpdateAsync(x => x.Id == outboxId,
                 x => x.SetProperty(y => y.Email, entity.Email)
                 .SetProperty(y => y.Name, entity.Name)
                 .SetProperty(y => y.SmtpHost, entity.SmtpHost)
                 .SetProperty(y => y.SmtpPort, entity.SmtpPort)
                 .SetProperty(y => y.UserName, entity.UserName)
                 .SetProperty(y => y.Password, entity.Password)
                 .SetProperty(y => y.EnableSSL, entity.EnableSSL)
                 .SetProperty(y => y.Description, entity.Description)
                 .SetProperty(y => y.ProxyId, entity.ProxyId)
                 .SetProperty(y => y.ReplyToEmails, entity.ReplyToEmails)
                 );
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 测试发件箱是否可用
        /// </summary>
        /// <param name="outboxId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpPut("outbox/{outboxId:long}/validation")]
        public async Task<ResponseResult<bool>> ValidateOutbox(long outboxId, [FromBody] SmtpPasswordSecretKeys smtpPasswordSecretKeys)
        {
            // 只能测试属于自己的发件箱
            var userId = tokenService.GetUserDataId();

            var outbox = await db.Outboxes.FirstOrDefaultAsync(x => x.Id == outboxId && x.UserId == userId) ?? throw new KnownException("发件箱不存在");

            // 发送测试邮件
            var outboxTestor = new OutboxTestSender(outbox, smtpPasswordSecretKeys, db);
            var result = await outboxTestor.SendTest();

            // 更新数据库
            await db.Outboxes.UpdateAsync(x => x.Id == outboxId, x => x.SetProperty(y => y.IsValid, result.Ok)
            .SetProperty(x => x.ValidFailReason, result.Message));

            return new ResponseResult<bool>()
            {
                Ok = true,
                Data = result.Ok,
                Message = result.Message,
            };
        }

        /// <summary>
        /// 更新收件箱
        /// </summary>
        /// <param name="inboxId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("inbox/{inboxId:long}")]
        public async Task<ResponseResult<bool>> UpdateInbox(long inboxId, [FromBody] Inbox entity)
        {
            await db.Inboxes.UpdateAsync(x => x.Id == inboxId,
                 x => x.SetProperty(y => y.Email, entity.Email)
                    .SetProperty(y => y.Name, entity.Name)
                    .SetProperty(y => y.MinInboxCooldownHours, entity.MinInboxCooldownHours)
                    .SetProperty(y => y.Description, entity.Description)
                 );
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮箱数量
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="emailBoxType"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("outbox/filtered-count")]
        public async Task<ResponseResult<int>> GetOutboxesCount(long groupId, string filter)
        {
            var userId = tokenService.GetUserDataId();

            // 收件箱
            var dbSet = db.Outboxes.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden);
            if (groupId > 0)
            {
                dbSet = dbSet.Where(x => x.EmailGroupId == groupId);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Email.Contains(filter) || x.Description.Contains(filter));
            }
            int count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮箱数据
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="emailBoxType"></param>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("outbox/filtered-data")]
        public async Task<ResponseResult<List<Outbox>>> GetOutboxesData(long groupId, string filter, [FromBody] Pagination pagination)
        {
            var userId = tokenService.GetUserDataId();
            var dbSet = db.Outboxes.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden);
            if (groupId > 0)
            {
                dbSet = dbSet.Where(x => x.EmailGroupId == groupId);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Email.Contains(filter) || x.Description.Contains(filter));
            }
            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 通过 id 删除邮箱
        /// 若邮箱在使用，则仅标记一个删除状态
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpDelete("outboxes/{emailBoxId:long}")]
        public async Task<ResponseResult<bool>> DeleteOutboxById(long emailBoxId)
        {
            var emailBox = await db.Outboxes.FirstOrDefaultAsync(x => x.Id == emailBoxId);
            if (emailBox == null) throw new KnownException("邮箱不存在");
            db.Outboxes.Remove(emailBox);
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮箱数量
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="emailBoxType"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("inbox/filtered-count")]
        public async Task<ResponseResult<int>> GetInboxesCount(long groupId, string filter)
        {
            var userId = tokenService.GetUserDataId();

            // 收件箱
            var dbSet = db.Inboxes.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden);
            if (groupId > 0)
            {
                dbSet = dbSet.Where(x => x.EmailGroupId == groupId);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Email.Contains(filter) || x.Description.Contains(filter));
            }
            int count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮箱数据
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="emailBoxType"></param>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("inbox/filtered-data")]
        public async Task<ResponseResult<List<Inbox>>> GetInboxesData(long groupId, string filter, [FromBody] Pagination pagination)
        {
            var userId = tokenService.GetUserDataId();
            var dbSet = db.Inboxes.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden);
            if (groupId > 0)
            {
                dbSet = dbSet.Where(x => x.EmailGroupId == groupId);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Email.Contains(filter) || x.Description.Contains(filter));
            }
            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 通过 id 删除邮箱
        /// 若邮箱在使用，则仅标记一个删除状态
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpDelete("inboxes/{emailBoxId:long}")]
        public async Task<ResponseResult<bool>> DeleteInboxById(long emailBoxId)
        {
            var emailBox = await db.Inboxes.FirstOrDefaultAsync(x => x.Id == emailBoxId) ?? throw new KnownException("邮箱不存在");
            emailBox.IsDeleted = true;
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
