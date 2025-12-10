using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Controllers.Emails.Models;
using UZonMail.CorePlugin.Services.EmailDecorator;
using UZonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.Emails
{
    /// <summary>
    /// 发件相关接口
    /// </summary>
    public class EmailSendingController(
        SqlContext db,
        SendingGroupService sendingService,
        TokenService tokenService,
        EmailContentDecorateService decorateService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 预览发件项
        /// </summary>
        /// <returns></returns>
        [HttpPost("preview")]
        public async Task<ResponseResult<SendingItemPreview>> PreviewSendingItem(
            [FromBody] SendingItemPreview data
        )
        {
            // 对数据进行替换
            var userId = tokenService.GetUserSqlId();

            var inbox =
                await db
                    .Inboxes.Where(x => x.UserId == userId && x.Email == data.Inbox)
                    .Select(x => new EmailAddress() { Email = x.Email, Name = x.Name })
                    .FirstOrDefaultAsync()
                ?? new EmailAddress() { Email = data.Inbox, Name = "inbox-name-preview" };

            // 获取指定的 outbox
            var outboxEmail = data.Data.SelectTokenOrDefault("outbox", data.Outbox);
            var outbox =
                await db
                    .Outboxes.Where(x => x.UserId == userId && x.Email == data.Outbox)
                    .FirstOrDefaultAsync()
                ?? new Outbox() { Email = "outbox-preview@test.com", Name = "outbox-name" };

            // 允许通过 data 参数覆盖 outbox name
            var outboxNameFromData = data.Data.SelectTokenOrDefault("outboxName", string.Empty);
            if (!string.IsNullOrEmpty(outboxNameFromData))
                outbox.Name = outboxNameFromData;
            var inboxNameFromData = data.Data.SelectTokenOrDefault("inboxName", string.Empty);
            if (!string.IsNullOrEmpty(inboxNameFromData))
                inbox.Name = inboxNameFromData;

            var sendingItem = new SendingItem()
            {
                UserId = userId,
                Subject = data.Subject,
                Content = data.Body,
                Data = data.Data,
                Inboxes = [inbox]
            };

            var sendItemMeta = new SendItemMeta(0);
            sendItemMeta.SetSendingItem(sendingItem);
            var decoratorParams = new EmailDecoratorParams(
                new SendingSetting(),
                sendItemMeta,
                outbox
            );
            data.Subject = await decorateService.ResolveVariables(decoratorParams, data.Subject);
            data.Body = await decorateService.ResolveVariables(decoratorParams, data.Body);
            return data.ToSuccessResponse();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [HttpPost("now")]
        public async Task<ResponseResult<SendingGroup>> SendNow([FromBody] SendingGroup sendingData)
        {
            sendingData.ScheduleDate = DateTime.MinValue;
            return await sendingService.StartSending(sendingData);
        }

        /// <summary>
        /// 计划发送
        /// </summary>
        /// <param name="sendingData"></param>
        /// <returns></returns>
        [HttpPost("schedule")]
        public async Task<ResponseResult<SendingGroup>> SendSchedule(
            [FromBody] SendingGroup sendingData
        )
        {
            // 校验数据
            return await sendingService.StartSending(sendingData);
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
            var sendingGroup = await db
                .SendingGroups.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sendingGroupId);
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }

            // 移除发件组任务
            await sendingService.RemoveSendingGroupTask(sendingGroup);
            await sendingService.UpdateSendingGroupStatus(
                sendingGroup.Id,
                SendingGroupStatus.Pause,
                "手动暂停"
            );

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 重新开始发件
        /// 该接口同时支持取消的重新发件
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpPost("sending-groups/{sendingGroupId:long}/restart")]
        public async Task<ResponseResult<bool>> RestartSending(long sendingGroupId)
        {
            // 查找发件组
            var sendingGroup = await db
                .SendingGroups.AsNoTracking()
                .Where(x => x.Id == sendingGroupId)
                .FirstOrDefaultAsync();
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }

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
            var sendingGroup = await db.SendingGroups.FirstOrDefaultAsync(x =>
                x.Id == sendingGroupId
            );
            if (sendingGroup == null)
            {
                return false.ToFailResponse("发件组不存在");
            }

            await sendingService.RemoveSendingGroupTask(sendingGroup);
            await sendingService.UpdateSendingGroupStatus(
                sendingGroup.Id,
                SendingGroupStatus.Pause,
                "手动取消"
            );

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 重新发送某一封邮件
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <returns></returns>
        [HttpPost("sending-items/{sendingItemId:long}/resend")]
        public async Task<ResponseResult<bool>> ResendSendingItem(long sendingItemId)
        {
            var sendingItem = await db
                .SendingItems.Where(x => x.Id == sendingItemId)
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

            await sendingService.SendNow(sendingGroup, [sendingItem.Id]);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 重新发送整个发件组
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        [HttpPost("sending-groups/{sendingGroupId:long}/resend")]
        public async Task<ResponseResult<bool>> ResendSendingGroup(long sendingGroupId)
        {
            // 查找发件项
            var sendingGroup = await db.SendingGroups.FirstOrDefaultAsync(x =>
                x.Id == sendingGroupId
            );
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
            await sendingService.SendNow(sendingGroup);
            return true.ToSuccessResponse();
        }
    }
}
