using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Statistics.Model;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Statistics
{
    public class StatisticsController(SqlContext db, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取发件账户统计信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("sender-accounts")]
        public async Task<ResponseResult<List<EmailCount>>> GetSenderEmailCountInfo()
        {
            var userId = tokenService.GetUserSqlId();
            var emailCounts = await db
                .SenderAccounts.Where(x => x.EmailAccount.UserId == userId)
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.EmailAccount.Domain)
                .Select(x => new EmailCount { Domain = x.Key ?? string.Empty, Count = x.Count() })
                .ToListAsync();
            return emailCounts.ToSuccessResponse();
        }

        /// <summary>
        /// 获取收件联系人统计信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("recipient-contacts")]
        public async Task<ResponseResult<List<EmailCount>>> GetRecipientsEmailCountInfo()
        {
            var userId = tokenService.GetUserSqlId();
            var emailCounts = await db
                .RecipientContacts.Where(x => x.UserId == userId)
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Domain)
                .Select(x => new EmailCount { Domain = x.Key ?? string.Empty, Count = x.Count() })
                .ToListAsync();
            return emailCounts.ToSuccessResponse();
        }

        /// <summary>
        /// 每月发送邮件统计
        /// </summary>
        /// <returns></returns>
        [HttpGet("monthly-sending")]
        public async Task<ResponseResult<List<MonthlySendingInfo>>> GetMonthlySendingCountInfo()
        {
            var userId = tokenService.GetUserSqlId();
            var monthlySendingInfos = await db
                .SendingItems.OfType<SendingItem>()
                .Where(x => x.UserId == userId)
                .Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.CreateDate.Year, x.CreateDate.Month })
                .Select(x => new MonthlySendingInfo
                {
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    Count = x.Count()
                })
                .ToListAsync();

            return monthlySendingInfos.ToSuccessResponse();
        }
    }
}
