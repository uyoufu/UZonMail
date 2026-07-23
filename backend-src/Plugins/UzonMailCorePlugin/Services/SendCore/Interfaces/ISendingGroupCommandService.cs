using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingGroupCommandService
    {
        Task<ResponseResult<SendingGroup>> StartSending(SendingGroup sendingData);

        Task SendNow(SendingGroup sendingGroup, List<long>? sendItemIds = null);

        Task RemoveSendingGroupTask(SendingGroup sendingGroup, string removeReason);
    }
}
