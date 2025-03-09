using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Emails;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Emails
{
    public class EmailUtilsService(SqlContext db, TokenService tokenService) : IScopedService
    {
        /// <summary>
        /// 验证发件箱是否有效
        /// </summary>
        /// <param name="outboxId"></param>
        /// <param name="smtpPasswordSecretKeys"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<ResponseResult<bool>> ValidateOutbox(long outboxId, SmtpPasswordSecretKeys smtpPasswordSecretKeys)
        {
            // 只能测试属于自己的发件箱
            var userId = tokenService.GetUserSqlId();

            var outbox = await db.Outboxes.FirstOrDefaultAsync(x => x.Id == outboxId && x.UserId == userId) ?? throw new KnownException("发件箱不存在");
            var result = await ValidateOutbox(outbox, smtpPasswordSecretKeys);
            return result;
        }

        public async Task<ResponseResult<bool>> ValidateOutbox(Outbox outbox, SmtpPasswordSecretKeys smtpPasswordSecretKeys)
        {
            // 发送测试邮件
            var outboxTestor = new OutboxTestSender(outbox, smtpPasswordSecretKeys, db);
            var result = await outboxTestor.SendTest();

            // 更新数据库
            await db.Outboxes.UpdateAsync(x => x.Id == outbox.Id,
                x => x.SetProperty(y => y.IsValid, result.Ok)
                .SetProperty(y => y.Status, result.Ok ? OutboxStatus.Valid : OutboxStatus.Invalid)
                .SetProperty(x => x.ValidFailReason, result.Message));

            return new ResponseResult<bool>()
            {
                Ok = result.Ok,
                Data = result.Ok,
                Message = result.Message,
            };
        }
    }
}
