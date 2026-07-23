using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingScheduleService
    {
        Task ScheduleSending(SendingGroup sendingGroup);

        Task RemoveSendSchedule(long sendingGroupId);
    }
}
