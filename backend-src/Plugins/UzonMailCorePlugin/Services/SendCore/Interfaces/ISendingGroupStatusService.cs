using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingGroupStatusService
    {
        Task UpdateSendingGroupStatus(
            long sendingGroupId,
            SendingGroupStatus status,
            string updateReason = ""
        );

        Task UpdateSendingGroupStatus(
            List<long> sendingGroupIds,
            SendingGroupStatus status,
            string updateReason = ""
        );
    }
}
