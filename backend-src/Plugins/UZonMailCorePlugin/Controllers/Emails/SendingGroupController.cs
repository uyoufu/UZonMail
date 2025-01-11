using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Emails.Models;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.Utils.Web.PagingQuery;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
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
            var dbSet = db.SendingGroups.AsNoTracking().Where(x => x.UserId == userId);
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
        public async Task<ResponseResult<List<SendingHistoryResult>>> GetSendingGroupsData(string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db.SendingGroups.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Subjects.Contains(filter));
            }
            dbSet = dbSet.Include(x => x.Templates).Include(x => x.Outboxes)
                .Select(x => new SendingGroup()
                {
                    Id = x.Id,
                    Subjects = x.Subjects,
                    SendingType = x.SendingType,
                    Status = x.Status,
                    Templates = x.Templates,
                    Outboxes = x.Outboxes, // 兼容旧数据
                    OutboxesCount = x.OutboxesCount,
                    InboxesCount = x.InboxesCount,
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
            var results = await db.SendingGroups.Where(x => x.Status == SendingGroupStatus.Sending).ToListAsync();
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
            var result = await db.SendingGroups.FirstOrDefaultAsync(x => x.Id == sendingGroupId && x.UserId == userId);
            if (result == null) return new ErrorResponse<string>("邮箱组不存在");
            return result.Subjects.ToSuccessResponse();
        }

        /// <summary>
        /// 获取正在执行发送任务的邮件组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpGet("{sendingGroupId:long}/status-info")]
        public async Task<ResponseResult<SendingGroupStatusInfo>> GetSendingGroupRunningInfo(long sendingGroupId)
        {
            var userId = tokenService.GetUserSqlId();
            var result = await db.SendingGroups.FirstOrDefaultAsync(x => x.Id == sendingGroupId && x.UserId == userId);
            if (result == null) return new ErrorResponse<SendingGroupStatusInfo>("邮箱组不存在");
            return new SendingGroupStatusInfo(result).ToSuccessResponse();
        }
    }
}
