using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Emails
{
    /// <summary>
    /// 邮箱验证服务
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tokenService"></param>
    /// <param name="debugConfig"></param>
    public class OutboxValidateService(IServiceProvider serviceProvider, SqlContext db, TokenService tokenService, DebugConfig debugConfig, EmailSendersManager sendersManager) : IScopedService
    {
        /// <summary>
        /// 验证发件箱是否有效
        /// </summary>
        /// <param name="outboxId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<ResponseResult<bool>> ValidateOutbox(long outboxId)
        {
            // 只能测试属于自己的发件箱
            var userId = tokenService.GetUserSqlId();

            var outbox = await db.Outboxes.FirstOrDefaultAsync(x => x.Id == outboxId && x.UserId == userId) ?? throw new KnownException("发件箱不存在");
            var result = await ValidateOutbox(outbox);
            return result;
        }

        /// <summary>
        /// 验证发件箱是否有效
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public async Task<ResponseResult<bool>> ValidateOutbox(Outbox outbox)
        {
            var emailSender = sendersManager.GetEmailSender(outbox.Email);
            var result = await emailSender.TestOutbox(serviceProvider, outbox);

            // 更新数据库
            await db.Outboxes.UpdateAsync(x => x.Id == outbox.Id,
                x => x.SetProperty(y => y.IsValid, result.Ok)
                .SetProperty(y => y.Status, result.Ok ? OutboxStatus.Valid : OutboxStatus.Invalid)
                .SetProperty(x => x.ValidFailReason, result.Message));

            return new ResponseResult<bool>()
            {
                Ok = result.Ok,
                Data = result.Ok,
                Message = $"[{outbox.Email}] {result.Message}",
            };
        }
    }
}
