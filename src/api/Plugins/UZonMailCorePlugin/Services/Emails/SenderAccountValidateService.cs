using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.Users.Model;
using UzonMail.CorePlugin.Services.SendCore.Sender;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.ResponseModel;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Emails
{
    /// <summary>
    /// 邮箱验证服务
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tokenService"></param>
    public class SenderAccountValidateService(
        IServiceProvider serviceProvider,
        SqlContext db,
        TokenService tokenService,
        EmailSendersManager sendersManager,
        SenderAccountRuntimeFactory runtimeFactory
    ) : IScopedService
    {
        /// <summary>
        /// 验证发件箱是否有效
        /// </summary>
        /// <param name="senderAccountId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<ResponseResult<bool>> ValidateSenderAccount(long senderAccountId)
        {
            // 只能测试属于自己的发件箱
            var userId = tokenService.GetUserSqlId();

            var senderAccount =
                await db
                    .SenderAccounts.Include(x => x.EmailAccount)
                    .ThenInclude(x => x.OAuthCredential)
                    .Include(x => x.SmtpCredential)
                    .FirstOrDefaultAsync(x =>
                        x.Id == senderAccountId && x.EmailAccount.UserId == userId
                    ) ?? throw new KnownException("发件箱不存在");
            var result = await ValidateSenderAccount(senderAccount);
            return result;
        }

        /// <summary>
        /// 验证发件箱是否有效
        /// </summary>
        /// <param name="senderAccount"></param>
        /// <returns></returns>
        public async Task<ResponseResult<bool>> ValidateSenderAccount(SenderAccount senderAccount)
        {
            if (senderAccount.EmailAccount == null)
            {
                senderAccount =
                    await db
                        .SenderAccounts.Include(x => x.EmailAccount)
                        .ThenInclude(x => x.OAuthCredential)
                        .Include(x => x.SmtpCredential)
                        .FirstOrDefaultAsync(x => x.Id == senderAccount.Id)
                    ?? throw new KnownException("发件账户不存在");
            }

            var runtimeAccount = runtimeFactory.Create(
                senderAccount,
                0,
                SenderEmailAddressType.Shared
            );
            var emailSender = sendersManager.GetEmailSender(senderAccount.Protocol);
            var result = await emailSender.ValidateAsync(serviceProvider, runtimeAccount);

            // 更新数据库
            await db.SenderAccounts.UpdateAsync(
                x => x.Id == senderAccount.Id,
                x =>
                    x.SetProperty(
                            y => y.Status,
                            result.IsSuccess
                                ? SenderAccountStatus.Valid
                                : SenderAccountStatus.Invalid
                        )
                        .SetProperty(x => x.ValidationFailureReason, result.Message)
            );

            return new ResponseResult<bool>()
            {
                Ok = result.IsSuccess,
                Data = result.IsSuccess,
                Message = $"[{senderAccount.Email}] {result.Message}",
            };
        }
    }
}
