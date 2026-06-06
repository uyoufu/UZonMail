using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingGroupCreationService
    {
        Task<SendingGroup> CreateSendingGroup(SendingGroup sendingGroupData);
    }
}
