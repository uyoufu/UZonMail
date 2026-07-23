using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.ResponseModel;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    /// <summary>
    /// Compatibility facade for existing callers. New code should depend on the SendCore interfaces.
    /// </summary>
    public class SendingGroupService(
        ISendingGroupCreationService creationService,
        ISendingGroupCommandService commandService,
        ISendingGroupStatusService statusService,
        ISendingScheduleService scheduleService
    ) : IScopedService
    {
        public Task<SendingGroup> CreateSendingGroup(SendingGroup sendingGroupData) =>
            creationService.CreateSendingGroup(sendingGroupData);

        public Task SendNow(SendingGroup sendingGroup, List<long>? sendItemIds = null) =>
            commandService.SendNow(sendingGroup, sendItemIds);

        public Task SendSchedule(SendingGroup sendingGroup) =>
            scheduleService.ScheduleSending(sendingGroup);

        public Task<ResponseResult<SendingGroup>> StartSending(SendingGroup sendingData) =>
            commandService.StartSending(sendingData);

        public Task RemoveSendingGroupTask(SendingGroup sendingGroup, string removeReason) =>
            commandService.RemoveSendingGroupTask(sendingGroup, removeReason);

        public Task UpdateSendingGroupStatus(
            long sendingGroupId,
            SendingGroupStatus status,
            string updateReason = ""
        ) => statusService.UpdateSendingGroupStatus(sendingGroupId, status, updateReason);

        public Task UpdateSendingGroupStatus(
            List<long> sendingGroupIds,
            SendingGroupStatus status,
            string updateReason = ""
        ) => statusService.UpdateSendingGroupStatus(sendingGroupIds, status, updateReason);

        public Task RemoveSendSchedule(long sendingGroupId) =>
            scheduleService.RemoveSendSchedule(sendingGroupId);
    }
}
