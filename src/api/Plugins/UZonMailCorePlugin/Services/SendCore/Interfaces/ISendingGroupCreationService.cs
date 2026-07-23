using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingGroupCreationService
    {
        Task<SendingGroup> CreateSendingGroup(SendingGroup sendingGroupData);
    }
}
