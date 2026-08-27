using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Controllers.Emails.Models;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails
{
    /// <summary>
    /// 发件历史
    /// </summary>
    /// <param name="tokenService"></param>
    public class SendingGroupController(SqlContext db, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取邮件模板数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetSendingGroupsCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db
                .SendingGroups.AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Subjects.Contains(filter));
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
        public async Task<ResponseResult<List<SendingHistoryResult>>> GetSendingGroupsData(
            string filter,
            Pagination pagination
        )
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db
                .SendingGroups.AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Subjects.Contains(filter));
            }
            dbSet = dbSet
                .Include(x => x.Templates)
                .Include(x => x.SenderAccounts)
                .Select(x => new SendingGroup()
                {
                    Id = x.Id,
                    ObjectId = x.ObjectId,
                    Subjects = x.Subjects,
                    SendingType = x.SendingType,
                    Status = x.Status,
                    StatusReason = x.StatusReason,
                    ResumeAtUtc = x.ResumeAtUtc,
                    Templates = x.Templates,
                    SenderAccounts = x.SenderAccounts,
                    SenderAccountCount = x.SenderAccountCount,
                    RecipientCount = x.RecipientCount,
                    SuccessCount = x.SuccessCount,
                    SentCount = x.SentCount,
                    CreateDate = x.CreateDate,
                    TotalCount = x.TotalCount,
                    ScheduleDate = x.ScheduleDate,
                });

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.Select(x => new SendingHistoryResult(x)).ToList().ToSuccessResponse();
        }

        /// <summary>
        /// 获取正在执行发送任务的邮件组
        /// </summary>
        /// <returns></returns>
        [HttpGet("running")]
        public async Task<ResponseResult<List<RunningSendingGroupResult>>> GetRunningSendingGroups()
        {
            var userId = tokenService.GetUserSqlId();
            var results = await db
                .SendingGroups.Where(x =>
                    x.UserId == userId
                    && (
                        x.Status == SendingGroupStatus.Sending
                        || x.Status == SendingGroupStatus.WaitingForQuotaReset
                    )
                )
                .ToListAsync();
            return results.ConvertAll(x => new RunningSendingGroupResult(x)).ToSuccessResponse();
        }

        /// <summary>
        /// 获取正在执行发送任务的邮件组
        /// </summary>
        /// <returns></returns>
        [HttpGet("{sendingGroupId:long}/subjects")]
        public async Task<ResponseResult<string>> GetSendingGroupSubjects(long sendingGroupId)
        {
            var userId = tokenService.GetUserSqlId();
            var result = await db.SendingGroups.FirstOrDefaultAsync(x =>
                x.Id == sendingGroupId && x.UserId == userId
            );
            if (result == null)
                return new ErrorResponse<string>("邮箱组不存在");
            return result.Subjects.ToSuccessResponse();
        }

        /// <summary>
        /// 获取正在执行发送任务的邮件组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpGet("{sendingGroupId:long}/status-info")]
        public async Task<ResponseResult<SendingGroupStatusInfo>> GetSendingGroupRunningInfo(
            long sendingGroupId
        )
        {
            var userId = tokenService.GetUserSqlId();
            var result = await db.SendingGroups.FirstOrDefaultAsync(x =>
                x.Id == sendingGroupId && x.UserId == userId
            );
            if (result == null)
                return new ErrorResponse<SendingGroupStatusInfo>("邮箱组不存在");
            return new SendingGroupStatusInfo(result).ToSuccessResponse();
        }

        /// <summary>
        /// 获取发件组的原始数据
        /// </summary>
        /// <param name="sendingGroupObjId"></param>
        /// <returns></returns>
        [HttpGet("{sendingGroupObjId:length(24)}")]
        public async Task<ResponseResult<SendingGroupTemplateDto>> GetSendingGroup(
            string sendingGroupObjId
        )
        {
            var userId = tokenService.GetUserSqlId();
            // 只能获取自己的发件组数据
            var sendingGroup = await db
                .SendingGroups.Where(x => x.UserId == userId && x.ObjectId == sendingGroupObjId)
                .Include(x => x.SenderAccounts)
                .ThenInclude(x => x.EmailAccount)
                .Include(x => x.Templates)
                .Include(x => x.Attachments!)
                .ThenInclude(x => x.FileObject)
                .FirstOrDefaultAsync();
            if (sendingGroup == null)
                return ResponseResult<SendingGroupTemplateDto>.Fail("未找到发件组模板");

            return SendingGroupTemplateDto.FromEntity(sendingGroup).ToSuccessResponse();
        }

        /// <summary>
        /// 批量删除发件组
        /// </summary>
        /// <returns></returns>
        [HttpDelete("ids/many")]
        public async Task<ResponseResult<bool>> DeleteSendingGroups(
            [FromBody] List<long> sendingGroupIds
        )
        {
            var userId = tokenService.GetUserSqlId();
            await db
                .SendingGroups.Where(x => x.UserId == userId)
                .Where(x => sendingGroupIds.Contains(x.Id))
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, true));

            return true.ToSuccessResponse();
        }
    }
}
