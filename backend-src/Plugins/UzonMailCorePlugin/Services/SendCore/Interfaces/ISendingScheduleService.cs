using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingScheduleService
    {
        Task ScheduleSending(SendingGroup sendingGroup);

        Task RemoveSendSchedule(long sendingGroupId);
    }
}
