using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Database.Validators;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Encrypt;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.UserInfos;
using UzonMail.CorePlugin.Utils.Extensions;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails
{
    /// <summary>
    /// 邮箱
    /// </summary>
    public class EmailBoxController(
        SqlContext db,
        TokenService tokenService,
        EmailGroupService emailGroupService,
        OutboxValidateService emailUtils,
        EncryptService encryptService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 创建发件箱
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("outbox")]
        public async Task<ResponseResult<Outbox>> CreateOutbox([FromBody] CreateOutboxDto request)
        {
            var data = request.ToEntity();
            var outboxValidator = new OutboxValidator();
            var vdResult = outboxValidator.Validate(data);
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<Outbox>();
            }

            // 设置默认端口
            if (data.SmtpPort == 0)
                data.SmtpPort = 25; // 默认端口

            var userId = tokenService.GetUserSqlId();
            // 验证发件箱是否存在，若存在，则复用原来的发件箱
            Outbox? existOne = db.Outboxes.SingleOrDefault(x =>
                x.UserId == userId && x.Email == data.Email
            );

            // 添加用户和加密密码
            data.UserId = userId;
            data.Password = encryptService.EncrytPassword(data.Password);

            if (existOne != null)
            {
                existOne.Type = data.Type;
                existOne.EmailGroupId = data.EmailGroupId;
                existOne.SmtpPort = data.SmtpPort;
                existOne.Password = data.Password;
                existOne.UserName = data.UserName;
                existOne.Description = data.Description;
                existOne.ProxyId = data.ProxyId;
                existOne.ReplyToEmails = data.ReplyToEmails;
                existOne.ConnectionSecurity = data.ConnectionSecurity;
                existOne.SetStatusNormal();
            }
            else
            {
                // 新建一个发件箱
                db.Outboxes.Add(data);
                existOne = data;
            }
            await db.SaveChangesAsync();

            return existOne.ToSuccessResponse();
        }

        /// <summary>
        /// 批量新增发件箱
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        [HttpPost("outboxes")]
        public async Task<ResponseResult<List<Outbox>>> CreateOutboxes(
            [FromBody] List<CreateOutboxDto>? requests
        )
        {
            if (requests == null)
            {
                return ResponseResult<List<Outbox>>.Fail("未能解析发件箱数据");
            }

            var entities = requests.ConvertAll(x => x.ToEntity());
            var userId = tokenService.GetUserSqlId();
            foreach (var entity in entities)
            {
                // 设置默认端口
                if (entity.SmtpPort == 0)
                    entity.SmtpPort = 25;
                // 设置用户
                entity.UserId = userId;
                // 加密密码
                entity.Password = encryptService.EncrytPassword(entity.Password);

                // 验证数据
                var outboxValidator = new OutboxValidator();
                var vdResult = outboxValidator.Validate(entity);
                if (!vdResult.IsValid)
                {
                    return vdResult.ToErrorResponse<List<Outbox>>();
                }
            }

            List<string> emails = entities.Select(x => x.Email).ToList();
            List<Outbox> existEmails = await db
                .Outboxes.Where(x => x.UserId == userId && emails.Contains(x.Email))
                .ToListAsync();
            List<Outbox> newEntities =
            [
                .. emails
                    .Except(existEmails.Select(x => x.Email))
                    .Select(x => entities.Find(e => e.Email == x))
                    .OfType<Outbox>()
            ];

            // 新建发件箱
            await db.Outboxes.AddRangeAsync(newEntities);

            // 更新现有的发件箱
            foreach (var entity in existEmails)
            {
                var newEntity = entities.Find(x => x.Email == entity.Email);
                if (newEntity != null)
                {
                    entity.Type = newEntity.Type;
                    entity.EmailGroupId = newEntity.EmailGroupId;
                    entity.SmtpPort = newEntity.SmtpPort;
                    entity.UserName = newEntity.UserName;
                    entity.Password = newEntity.Password; // 密码已经加密处理
                    //entity.EnableSSL = newEntity.EnableSSL;
                    entity.ConnectionSecurity = newEntity.ConnectionSecurity;
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
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("inbox")]
        public Task<ResponseResult<Inbox>> CreateInbox([FromBody] CreateInboxDto request) =>
            CreateInboxEntity(request.ToEntity());

        /// <summary>
        /// 添加未分组收件箱
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpPost("inbox/ungrouped")]
        public async Task<ResponseResult<Inbox>> CreateUngroupedInbox(
            [FromBody] CreateUngroupedInboxDto request
        )
        {
            // 获取未分组的组
            var defaultGroup = await emailGroupService.GetDefaultEmailGroup(EmailGroupType.InBox);
            return await CreateInboxEntity(request.ToEntity(defaultGroup.Id));
        }

        /// <summary>
        /// 批量新增发件箱
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        [HttpPost("inboxes")]
        public async Task<ResponseResult<List<Inbox>>> CreateInboxes(
            [FromBody] List<CreateInboxDto>? requests
        )
        {
            if (requests == null)
            {
                return ResponseResult<List<Inbox>>.Fail("未能解析收件箱数据");
            }

            var entities = requests.ConvertAll(x => x.ToEntity());
            var tokenPayloads = tokenService.GetTokenPayloads();
            var userId = tokenPayloads.UserId;
            foreach (var entity in entities)
            {
                ValidateInitialInboxStatus(entity.Status);
                // 设置用户
                entity.UserId = userId;
                entity.OrganizationId = tokenPayloads.OrganizationId;
                var inboxValidator = new InboxValidator();
                var vdResult = inboxValidator.Validate(entity);
                if (!vdResult.IsValid)
                    return vdResult.ToErrorResponse<List<Inbox>>();
            }

            List<string> emails = entities.Select(x => x.Email).ToList();
            var invalidEmails = await GetInvalidInboxEmailsInOrganization(
                tokenPayloads.OrganizationId,
                emails
            );
            foreach (var entity in entities.Where(x => invalidEmails.Contains(x.Email)))
            {
                entity.Status = InboxStatus.Invalid;
            }

            List<Inbox> existEmails = await db
                .Inboxes.IgnoreQueryFilters()
                .Where(x => x.UserId == userId && emails.Contains(x.Email))
                .ToListAsync();
            List<Inbox> newEntities =
            [
                .. emails
                    .Except(existEmails.Select(x => x.Email))
                    .Select(x => entities.Find(e => e.Email == x))
                    .OfType<Inbox>()
                    .Distinct()
            ];

            // 新建发件箱
            foreach (var entity in newEntities)
            {
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
                    entity.Status = newEntity.Status ?? entity.Status;
                    entity.SetStatusNormal();
                }
            }
            await db.SaveChangesAsync();

            var invalidInboxEmails = entities
                .Where(x => x.Status == InboxStatus.Invalid)
                .Select(x => x.Email)
                .ToList();
            await UpdateInboxStatusesInOrganization(
                tokenPayloads.OrganizationId,
                invalidInboxEmails,
                InboxStatus.Invalid
            );

            // 返回所有的结果
            List<Inbox> results = [.. existEmails, .. newEntities];
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 更新发件箱
        /// </summary>
        /// <param name="outboxId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("outbox/{outboxId:long}")]
        public async Task<ResponseResult<bool>> UpdateOutbox(
            long outboxId,
            [FromBody] UpdateOutboxDto request
        )
        {
            var entity = request.ToEntity();
            await db.Outboxes.UpdateAsync(
                x => x.Id == outboxId,
                x =>
                    x.SetProperty(y => y.Email, entity.Email)
                        .SetProperty(y => y.Name, entity.Name)
                        .SetProperty(y => y.Type, entity.Type)
                        .SetProperty(y => y.SmtpHost, entity.SmtpHost)
                        .SetProperty(y => y.SmtpPort, entity.SmtpPort)
                        .SetProperty(y => y.UserName, entity.UserName)
                        //.SetProperty(y => y.EnableSSL, entity.EnableSSL)
                        .SetProperty(y => y.ConnectionSecurity, entity.ConnectionSecurity)
                        .SetProperty(y => y.Description, entity.Description)
                        .SetProperty(y => y.ProxyId, entity.ProxyId)
                        .SetProperty(y => y.ReplyToEmails, entity.ReplyToEmails)
            );

            // 如果密码为以 * 开头，则不更新密码
            if (!string.IsNullOrEmpty(entity.Password) && !entity.Password.StartsWith("***"))
            {
                await db.Outboxes.UpdateAsync(
                    x => x.Id == outboxId,
                    x =>
                        x.SetProperty(
                            y => y.Password,
                            encryptService.EncrytPassword(entity.Password)
                        )
                );
            }
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 测试发件箱是否可用
        /// </summary>
        /// <param name="outboxId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpPut("outbox/{outboxId:long}/validation")]
        public async Task<ResponseResult<bool>> ValidateOutbox(long outboxId)
        {
            var result = await emailUtils.ValidateOutbox(outboxId);
            // 让前端自己处理错误
            result.Ok = true;
            return result;
        }

        /// <summary>
        /// 更新收件箱
        /// </summary>
        /// <param name="inboxId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("inbox/{inboxId:long}")]
        public async Task<ResponseResult<bool>> UpdateInbox(
            long inboxId,
            [FromBody] UpdateInboxDto request
        )
        {
            var entity = request.ToEntity();
            await db.Inboxes.UpdateAsync(
                x => x.Id == inboxId,
                x =>
                    x.SetProperty(y => y.Email, entity.Email)
                        .SetProperty(y => y.Name, entity.Name)
                        .SetProperty(y => y.MinInboxCooldownHours, entity.MinInboxCooldownHours)
                        .SetProperty(y => y.Description, entity.Description)
            );
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 批量标记当前用户选择的收件箱状态
        /// 同组织同地址的收件箱需要保持一致，避免不同用户对同一地址出现冲突结论
        /// </summary>
        /// <param name="request">待更新的收件箱与目标状态</param>
        /// <returns>更新是否完成</returns>
        [HttpPut("inboxes/status")]
        public async Task<ResponseResult<bool>> UpdateInboxesStatus(
            [FromBody] UpdateInboxesStatusDto request
        )
        {
            if (request.Status is not (InboxStatus.Invalid or InboxStatus.Valid))
                throw new KnownException("收件箱状态仅支持标记为有效或无效");

            var inboxIds = request.InboxIds.Where(id => id > 0).Distinct().ToList();
            if (inboxIds.Count == 0)
                throw new KnownException("请至少选择一个收件箱");

            var tokenPayloads = tokenService.GetTokenPayloads();
            var inboxEmails = await db
                .Inboxes.AsNoTracking()
                .Where(x => x.UserId == tokenPayloads.UserId && inboxIds.Contains(x.Id))
                .Select(x => x.Email)
                .ToListAsync();
            await UpdateInboxStatusesInOrganization(
                tokenPayloads.OrganizationId,
                inboxEmails,
                request.Status
            );

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 批量移动发件箱到指定分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("outboxes/group")]
        public Task<ResponseResult<bool>> MoveOutboxesToGroup(
            [FromBody] MoveEmailBoxesDto request
        ) => MoveEmailBoxesToGroup(db.Outboxes, request, EmailGroupType.OutBox);

        /// <summary>
        /// 批量移动收件箱到指定分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("inboxes/group")]
        public Task<ResponseResult<bool>> MoveInboxesToGroup(
            [FromBody] MoveEmailBoxesDto request
        ) => MoveEmailBoxesToGroup(db.Inboxes, request, EmailGroupType.InBox);

        /// <summary>
        /// 将当前用户指定类型的邮箱批量迁移到其可访问的目标分组
        /// </summary>
        private async Task<ResponseResult<bool>> MoveEmailBoxesToGroup<TEmailBox>(
            IQueryable<TEmailBox> emailBoxes,
            MoveEmailBoxesDto request,
            EmailGroupType targetGroupType
        )
            where TEmailBox : EmailBox
        {
            var emailBoxIds = request.EmailBoxIds.Where(id => id > 0).Distinct().ToArray();
            if (emailBoxIds.Length == 0)
                throw new KnownException("请至少选择一个邮箱");

            var userId = tokenService.GetUserSqlId();
            var hasTargetGroupAccess = await db.EmailGroups.AnyAsync(group =>
                group.Id == request.TargetGroupId
                && group.UserId == userId
                && group.Type == targetGroupType
            );
            if (!hasTargetGroupAccess)
                throw new KnownException("目标邮箱分组不存在或无权访问");

            await emailBoxes
                .Where(emailBox => emailBox.UserId == userId && emailBoxIds.Contains(emailBox.Id))
                .ExecuteUpdateAsync(update =>
                    update.SetProperty(emailBox => emailBox.EmailGroupId, request.TargetGroupId)
                );

            return true.ToSuccessResponse();
        }

        private async Task<ResponseResult<Inbox>> CreateInboxEntity(Inbox entity)
        {
            ValidateInitialInboxStatus(entity.Status);
            var inboxValidator = new InboxValidator();
            var vdResult = inboxValidator.Validate(entity);
            if (!vdResult.IsValid)
                return vdResult.ToErrorResponse<Inbox>();

            var tokenPayloads = tokenService.GetTokenPayloads();
            var userId = tokenPayloads.UserId;
            entity.UserId = userId;
            entity.OrganizationId = tokenPayloads.OrganizationId;
            if (
                entity.Status == InboxStatus.Invalid
                || await IsInboxInvalidInOrganization(tokenPayloads.OrganizationId, entity.Email)
            )
            {
                entity.Status = InboxStatus.Invalid;
            }

            // 验证收件箱是否存在，若存在，则复用原来的收件箱。
            Inbox? existOne = db
                .Inboxes.IgnoreQueryFilters()
                .SingleOrDefault(x => x.UserId == userId && x.Email == entity.Email);
            if (existOne != null)
            {
                existOne.EmailGroupId = entity.EmailGroupId;
                existOne.Name = entity.Name;
                existOne.Description = entity.Description;
                existOne.Status = entity.Status ?? existOne.Status;
                existOne.SetStatusNormal();
            }
            else
            {
                db.Inboxes.Add(entity);
                existOne = entity;
            }
            await db.SaveChangesAsync();

            if (existOne.Status == InboxStatus.Invalid)
            {
                await UpdateInboxStatusesInOrganization(
                    tokenPayloads.OrganizationId,
                    [existOne.Email],
                    InboxStatus.Invalid
                );
            }

            return existOne.ToSuccessResponse();
        }

        private static void ValidateInitialInboxStatus(InboxStatus? status)
        {
            if (status is not null && status != InboxStatus.Invalid)
                throw new KnownException("新增收件箱仅支持指定无效状态");
        }

        private Task<bool> IsInboxInvalidInOrganization(long organizationId, string email) =>
            db
                .Inboxes.IgnoreQueryFilters()
                .AnyAsync(x =>
                    x.OrganizationId == organizationId
                    && x.Email == email
                    && x.Status == InboxStatus.Invalid
                );

        private async Task<HashSet<string>> GetInvalidInboxEmailsInOrganization(
            long organizationId,
            List<string> emails
        )
        {
            if (emails.Count == 0)
                return [];

            var invalidEmails = await db
                .Inboxes.IgnoreQueryFilters()
                .Where(x =>
                    x.OrganizationId == organizationId
                    && x.Status == InboxStatus.Invalid
                    && emails.Contains(x.Email)
                )
                .Select(x => x.Email)
                .ToListAsync();
            return invalidEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private async Task UpdateInboxStatusesInOrganization(
            long organizationId,
            List<string> emails,
            InboxStatus status
        )
        {
            var distinctEmails = emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (distinctEmails.Count == 0)
                return;

            await db
                .Inboxes.IgnoreQueryFilters()
                .Where(x => x.OrganizationId == organizationId && distinctEmails.Contains(x.Email))
                .ExecuteUpdateAsync(update =>
                    update
                        .SetProperty(x => x.Status, status)
                        .SetProperty(x => x.ValidFailReason, (string?)null)
                );
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
            if (groupId <= 0)
                return 0.ToSuccessResponse();

            var userId = tokenService.GetUserSqlId();

            var dbSet = db
                .Outboxes.AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden)
                .Where(x => x.EmailGroupId == groupId);

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Email.Contains(filter) || (x.Description ?? string.Empty).Contains(filter)
                );
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
        public async Task<ResponseResult<List<Outbox>>> GetOutboxesData(
            long groupId,
            string filter,
            [FromBody] Pagination pagination
        )
        {
            if (groupId <= 0)
                return ResponseResult<List<Outbox>>.Success([]);

            var userId = tokenService.GetUserSqlId();
            var dbSet = db
                .Outboxes.AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden)
                .Where(x => x.EmailGroupId == groupId);

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Email.Contains(filter) || (x.Description ?? string.Empty).Contains(filter)
                );
            }
            var results = await dbSet.Page(pagination).ToListAsync();

            // 将密码转为 6 个 * 号返回
            results.ForEach(x => x.Password = "******");

            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取组中发件邮箱的数据
        /// 仅返回 Id, Name, Email 三个字段
        /// </summary>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        [HttpGet("outbox/groups-data")]
        public async Task<ResponseResult<List<Outbox>>> GetGroupsOutboxes(string groupIds)
        {
            var longGroupIds = groupIds
                .Split(",")
                .Select(x =>
                {
                    if (long.TryParse(x, out var value))
                        return value;
                    return 0;
                })
                .Where(x => x > 0)
                .ToList();
            if (longGroupIds.Count == 0)
            {
                return ResponseResult<List<Outbox>>.Success([]);
            }

            var userId = tokenService.GetUserSqlId();
            var outboxes = await db
                .Outboxes.AsNoTracking()
                .Where(x => longGroupIds.Contains(x.EmailGroupId))
                .Where(x => x.UserId == userId)
                .Select(x => new Outbox()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email
                })
                .ToListAsync();

            return outboxes.ToSuccessResponse();
        }

        /// <summary>
        /// 获取发件箱信息
        /// </summary>
        /// <param name="outboxId"></param>
        /// <returns></returns>
        [HttpGet("outboxes/{outboxId:long}")]
        public async Task<ResponseResult<Outbox>> GetOutboxInfo(long outboxId)
        {
            var userId = tokenService.GetUserSqlId();
            var outbox = await db
                .Outboxes.Where(x => x.Id == outboxId && x.UserId == userId)
                .FirstOrDefaultAsync();
            if (outbox is null)
                return ResponseResult<Outbox>.Fail("未找到发件箱");

            return outbox.ToSuccessResponse();
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
            if (emailBox == null)
                throw new KnownException("邮箱不存在");
            db.Outboxes.Remove(emailBox);
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 批量删除多个发件箱
        /// </summary>
        /// <param name="emailBoxId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpDelete("outboxes/ids")]
        public async Task<ResponseResult<bool>> DeleteOutboxByIds([FromBody] List<string> outboxIds)
        {
            var userId = tokenService.GetUserSqlId();

            var emailBox = db.Outboxes.Where(x =>
                x.UserId == userId && outboxIds.Contains(x.ObjectId)
            );
            db.Outboxes.RemoveRange(emailBox);
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
            if (groupId <= 0)
                return 0.ToSuccessResponse();

            var userId = tokenService.GetUserSqlId();

            // 收件箱
            var dbSet = db
                .Inboxes.AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden)
                .Where(x => x.EmailGroupId == groupId);

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Email.Contains(filter) || (x.Description ?? string.Empty).Contains(filter)
                );
            }
            int count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取组中邮箱的数量
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet("inboxes/count")]
        public async Task<ResponseResult<int>> GetInboxesCountInGroups(string groupIds)
        {
            if (string.IsNullOrEmpty(groupIds))
                return 0.ToSuccessResponse();

            var groupIdsList = groupIds
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => long.Parse(x))
                .ToList();

            var userId = tokenService.GetUserSqlId();
            var count = await db
                .Inboxes.Where(x => x.UserId == userId && groupIdsList.Contains(x.EmailGroupId))
                .CountAsync();
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
        public async Task<ResponseResult<List<Inbox>>> GetInboxesData(
            long groupId,
            string filter,
            [FromBody] Pagination pagination
        )
        {
            if (groupId <= 0)
                return ResponseResult<List<Inbox>>.Success([]);

            var userId = tokenService.GetUserSqlId();
            var dbSet = db
                .Inboxes.AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsHidden)
                .Where(x => x.EmailGroupId == groupId);

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Email.Contains(filter) || (x.Description ?? string.Empty).Contains(filter)
                );
            }
            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 通过组获取收件箱
        /// </summary>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        [HttpGet("inbox/groups-data")]
        public async Task<ResponseResult<List<Inbox>>> GetGroupsInboxes(string groupIds)
        {
            var longGroupIds = groupIds
                .Split(",")
                .Select(x =>
                {
                    if (long.TryParse(x, out var value))
                        return value;
                    return 0;
                })
                .Where(x => x > 0)
                .ToList();
            if (longGroupIds.Count == 0)
            {
                return ResponseResult<List<Inbox>>.Success([]);
            }

            var userId = tokenService.GetUserSqlId();
            var inboxes = await db
                .Inboxes.AsNoTracking()
                .Where(x => longGroupIds.Contains(x.EmailGroupId))
                .Where(x => x.UserId == userId)
                .Select(x => new Inbox()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email
                })
                .ToListAsync();

            return inboxes.ToSuccessResponse();
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
            var emailBox =
                await db.Inboxes.FirstOrDefaultAsync(x => x.Id == emailBoxId)
                ?? throw new KnownException("邮箱不存在");
            emailBox.IsDeleted = true;
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// Delete all delivered inboxes in group of self
        /// </summary>
        /// <returns></returns>
        [HttpDelete("inboxes/groups/{groupId:long}/delivered")]
        public async Task<ResponseResult<bool>> DeleteAllDeliveredInboxesInGroup(long groupId)
        {
            var userId = tokenService.GetUserSqlId();
            await db
                .Inboxes.Where(x => x.UserId == userId)
                .Where(x => x.EmailGroupId == groupId)
                .Where(x => x.LastSuccessDeliveryDate > DateTime.MinValue)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, true));
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 批量删除多个收件箱
        /// </summary>
        /// <param name="inboxObjectIds"></param>
        /// <returns></returns>
        [HttpDelete("inboxes/ids")]
        public async Task<ResponseResult<bool>> DeleteInboxByIds(
            [FromBody] List<string> inboxObjectIds
        )
        {
            var userId = tokenService.GetUserSqlId();

            // 保持与单条删除一致，避免多选删除绕过收件箱的软删除和追溯能力
            await db
                .Inboxes.Where(x => x.UserId == userId && inboxObjectIds.Contains(x.ObjectId))
                .ExecuteUpdateAsync(x => x.SetProperty(inbox => inbox.IsDeleted, true));

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取加密后的发件箱密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet("encrypt-password")]
        public async Task<ResponseResult<string>> EncryptOutboxPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new KnownException("密码不能为空");

            var encryptedResult = encryptService.EncrytPassword(password);
            return encryptedResult.ToSuccessResponse();
        }
    }
}
