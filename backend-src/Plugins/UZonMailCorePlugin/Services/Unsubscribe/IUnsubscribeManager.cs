using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Unsubscribe
{
    /// <summary>
    /// 退订管理
    /// </summary>
    public interface IUnsubscribeManager
    {
        /// <summary>
        /// 获取所有退订的邮箱
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetUnsubscribedEmails(long organizationId);
    }
}
