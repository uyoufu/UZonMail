using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingGroupCommandService
    {
        Task<ResponseResult<SendingGroup>> StartSending(SendingGroup sendingData);

        Task SendNow(SendingGroup sendingGroup, List<long>? sendItemIds = null);

        Task RemoveSendingGroupTask(SendingGroup sendingGroup, string removeReason);
    }
}
