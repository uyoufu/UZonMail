using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Database.Validators;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.Utils.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.ResponseModel;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    public class SendingGroupCommandService(
        SqlContext db,
        DebugConfig debugConfig,
        ISendingGroupCreationService creationService,
        ISendingScheduleService scheduleService,
        ISendingWorkerCoordinator workerCoordinator,
        GroupTasksManager waitList,
        OutboxesManager outboxesManager,
        SmtpClientsManager clientFactory,
        IServiceProvider serviceProvider
    ) : ISendingGroupCommandService, IScopedService<ISendingGroupCommandService>
    {
        public async Task<ResponseResult<SendingGroup>> StartSending(SendingGroup sendingData)
        {
            var sendingGroupValidator = new SendingGroupValidator();
            var vdResult = sendingGroupValidator.Validate(sendingData);
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<SendingGroup>();
            }

            var scheduleDate =
                sendingData.ScheduleDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(sendingData.ScheduleDate, DateTimeKind.Local)
                    : sendingData.ScheduleDate;
            var isSchedule = scheduleDate.ToUniversalTime() > DateTime.UtcNow.AddMinutes(1);

            sendingData.SendingType = isSchedule
                ? SendingGroupType.Scheduled
                : SendingGroupType.Instant;
            var sendingGroup = await creationService.CreateSendingGroup(sendingData);

            if (isSchedule)
            {
                await scheduleService.ScheduleSending(sendingGroup);
            }
            else
            {
                await SendNow(sendingGroup);
            }

            return new SendingGroup()
            {
                Id = sendingGroup.Id,
                ObjectId = sendingGroup.ObjectId,
                TotalCount = sendingGroup.TotalCount
            }.ToSuccessResponse();
        }

        public async Task SendNow(SendingGroup sendingGroup, List<long>? sendItemIds = null)
        {
            if (debugConfig.IsDemo)
            {
                var sendingItemsCount = await db.SendingItems.CountAsync(x =>
                    x.SendingGroupId == sendingGroup.Id
                );
                if (sendingItemsCount > 5)
                {
                    throw new KnownException("示例环境最多群发 5 条");
                }
            }

            var sendingContext = serviceProvider.GetRequiredService<SendingContext>();
            await waitList.AddSendingGroup(sendingContext, sendingGroup, sendItemIds);
            await workerCoordinator.StartSendingAsync();
        }

        public async Task RemoveSendingGroupTask(SendingGroup sendingGroup, string removeReason)
        {
            if (sendingGroup.Status == SendingGroupStatus.Sending)
            {
                var removedOutboxes = outboxesManager.RemoveOutbox(
                    sendingGroup.Id,
                    removeReason
                );

                foreach (var outbox in removedOutboxes)
                {
                    await clientFactory.DisposeSmtpClientsAsync(outbox.Email);
                }

                waitList.RemoveSendingGroupTask(sendingGroup.UserId, sendingGroup.Id);
            }

            if (sendingGroup.SendingType == SendingGroupType.Scheduled)
            {
                await scheduleService.RemoveSendSchedule(sendingGroup.Id);
            }
        }
    }
}
