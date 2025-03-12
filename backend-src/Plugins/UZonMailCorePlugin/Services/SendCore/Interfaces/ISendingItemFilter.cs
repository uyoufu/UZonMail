using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Services.SendCore.Interfaces
{
    /// <summary>
    /// 自定义收件箱过滤器
    /// 每个插件可以将自己的过滤器实现注册到该接口，该接口会自动调用
    /// </summary>
    public interface ISendingItemFilter
    {
        /// <summary>
        /// 获取无效的发送项 id
        /// </summary>
        /// <param name="sendingItems"></param>
        /// <returns></returns>
        Task<List<long>> GetInvalidSendingItemIds(List<SendingItem> sendingItems);
    }
}
