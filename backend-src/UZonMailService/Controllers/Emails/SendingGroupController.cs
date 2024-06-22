﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.Extensions;
using Uamazing.Utils.Web.ResponseModel;
using UZonMailService.Controllers.Emails.Models;
using UZonMailService.Models.SQL;
using UZonMailService.Models.SQL.EmailSending;
using UZonMailService.Models.SQL.Templates;
using UZonMailService.Services.Settings;
using UZonMailService.Utils.ASPNETCore.PagingQuery;

namespace UZonMailService.Controllers.Emails
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
            var userId = tokenService.GetUserDataId();
            var dbSet = db.SendingGroups.Where(x => x.UserId == userId);
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
            var userId = tokenService.GetUserDataId();
            var dbSet = db.SendingGroups.Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Subjects.Contains(filter));
            }
            dbSet = dbSet.Include(x => x.Templates).Include(x => x.Outboxes);

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
            var userId = tokenService.GetUserDataId();
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
            var userId = tokenService.GetUserDataId();
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
            var userId = tokenService.GetUserDataId();
            var result = await db.SendingGroups.FirstOrDefaultAsync(x => x.Id == sendingGroupId && x.UserId == userId);
            if (result == null) return new ErrorResponse<SendingGroupStatusInfo>("邮箱组不存在");
            return new SendingGroupStatusInfo(result).ToSuccessResponse();
        }
    }
}
