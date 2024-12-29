using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Emails.Models;
using UZonMail.Core.Services.EmailSending;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.Core.Database.Validators;
using UZonMail.Core.Utils.Extensions;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// 发件相关接口
    /// </summary>
    public class EmailSendingController(SqlContext db
        , SendingGroupService sendingService
        ) : ControllerBaseV1
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("now")]
        public async Task<ResponseResult<SendingGroup>> SendNow([FromBody] SendingGroup sendingData)
        {
            var sendingGroupValidator = new SendingGroupValidator();
            var vdResult = sendingGroupValidator.Validate(sendingData);
            // 校验数据
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<SendingGroup>();
            }

            // 创建发件组
            sendingData.SendingType = SendingGroupType.Instant;
            var sendingGroup = await sendingService.CreateSendingGroup(sendingData);
            db.ChangeTracker.Clear();
            await sendingService.SendNow(sendingGroup);

            return new SendingGroup()
            {
                Id = sendingGroup.Id,
                TotalCount = sendingGroup.TotalCount
            }.ToSuccessResponse();
        }

        /// <summary>
        /// 计划发送
        /// </summary>
        /// <param name="sendingData"></param>
        /// <returns></returns>
        [HttpPost("schedule")]
        public async Task<ResponseResult<SendingGroup>> SendSchedule([FromBody] SendingGroup sendingData)
        {
            // 校验数据
            var sendingGroupValidator = new SendingGroupValidator();
            var vdResult = sendingGroupValidator.Validate(sendingData);
            // 校验数据
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<SendingGroup>();
            }

            sendingData.SendingType = SendingGroupType.Scheduled;

            // 创建发件组
            var sendingGroup = await sendingService.CreateSendingGroup(sendingData);
            await sendingService.SendSchedule(sendingGroup);

            return new SendingGroup()
            {
                Id = sendingGroup.Id,
                TotalCount = sendingGroup.TotalCount
            }.ToSuccessResponse();
        }

        /// <summary>
        /// 暂停发件
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpPost("sending-groups/{sendingGroupId:long}/pause")]
        public async Task<ResponseResult<bool>> PauseSending(long sendingGroupId)
        {
            // 查找发件组
            var sendingGroup = await db.SendingGroups.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sendingGroupId);
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }

            // 移除发件组任务
            sendingService.RemoveSendingGroupTaskOnly(sendingGroup);
            // 更新状态
            await db.SendingGroups.UpdateAsync(x => x.Id == sendingGroupId, x => x.SetProperty(y => y.Status, SendingGroupStatus.Pause));
            await db.SendingItems.UpdateAsync(x => x.SendingGroupId == sendingGroupId && x.Status == SendingItemStatus.Pending,
                x => x.SetProperty(y => y.Status, SendingItemStatus.Created));

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 重新开始发件
        /// 该接口同时支持取消的重新发件
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpPost("sending-groups/{sendingGroupId:long}/restart")]
        public async Task<ResponseResult<bool>> RestartSending(long sendingGroupId, [FromBody] SmtpSecretKeysModel smtpSecretKeys)
        {
            // 查找发件组
            var sendingGroup = await db.SendingGroups.AsNoTracking().Where(x => x.Id == sendingGroupId).FirstOrDefaultAsync();
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }            

            sendingGroup.SmtpPasswordSecretKeys = smtpSecretKeys.SmtpPasswordSecretKeys;

            // 重新开始发件
            await sendingService.SendNow(sendingGroup);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 取消发件
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpPost("sending-groups/{sendingGroupId:long}/cancel")]
        public async Task<ResponseResult<bool>> CancelSending(long sendingGroupId)
        {
            // 查找发件组
            var sendingGroup = await db.SendingGroups.FirstOrDefaultAsync(x => x.Id == sendingGroupId);
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }

            // 若处于发送中，则取消
            if (sendingGroup.Status == SendingGroupStatus.Sending)
            {
                // 取消发件
                sendingService.RemoveSendingGroupTaskOnly(sendingGroup);
            }

            // 更新状态
            await db.SendingGroups.UpdateAsync(x => x.Id == sendingGroupId, x => x.SetProperty(y => y.Status, SendingGroupStatus.Cancel));
            await db.SendingItems.UpdateAsync(x => x.SendingGroupId == sendingGroupId && x.Status == SendingItemStatus.Pending, x => x.SetProperty(y => y.Status, SendingItemStatus.Cancel));

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 重新发送某一封邮件
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <returns></returns>
        [HttpPost("sending-items/{sendingItemId:long}/resend")]
        public async Task<ResponseResult<bool>> ResendSendingItem(long sendingItemId, [FromBody] SmtpSecretKeysModel smtpSecretKeys)
        {
            var sendingItem = await db.SendingItems.Where(x => x.Id == sendingItemId)
                .Include(x => x.SendingGroup)
                .FirstOrDefaultAsync();
            if (sendingItem == null)
            {
                return false.ToFailResponse("发件项不存在");
            }

            // 查找发件项
            var sendingGroup = sendingItem.SendingGroup;
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }
            if (sendingGroup.SuccessCount == sendingGroup.TotalCount)
            {
                return false.ToFailResponse("发件组已全部成功，不支持重发");
            }

            // 开始发件
            sendingGroup.SmtpPasswordSecretKeys = smtpSecretKeys.SmtpPasswordSecretKeys;
            await sendingService.SendNow(sendingGroup, [sendingItem.Id]);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 重新发送整个发件组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpPost("sending-groups/{sendingGroupId:long}/resend")]
        public async Task<ResponseResult<bool>> ResendSendingGroup(long sendingGroupId, [FromBody] SmtpSecretKeysModel smtpSecretKeys)
        {
            // 查找发件项
            var sendingGroup = await db.SendingGroups.FirstOrDefaultAsync(x => x.Id == sendingGroupId);
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }
            if (sendingGroup.Status != SendingGroupStatus.Finish)
            {
                return false.ToFailResponse("发件组未结束，不支持重发");
            }
            if (sendingGroup.SuccessCount == sendingGroup.TotalCount)
            {
                return false.ToFailResponse("发件组已全部成功，不支持重发");
            }

            // 重新发送
            sendingGroup.SmtpPasswordSecretKeys = smtpSecretKeys.SmtpPasswordSecretKeys;
            await sendingService.SendNow(sendingGroup);
            return true.ToSuccessResponse();
        }
    }
}
