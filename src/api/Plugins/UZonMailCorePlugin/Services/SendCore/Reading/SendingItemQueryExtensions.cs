using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Reading;

/// <summary>
/// 发件项查询扩展
/// </summary>
public static class SendingItemQueryExtensions
{
    /// <summary>
    /// 排除已确认硬退信的发件项，避免再次投递到不可达收件箱
    /// </summary>
    public static IQueryable<SendingItem> ExcludeHardBounceItems(
        this IQueryable<SendingItem> sendingItems
    ) => sendingItems.Where(sendingItem => !sendingItem.IsHardBounce);
}
