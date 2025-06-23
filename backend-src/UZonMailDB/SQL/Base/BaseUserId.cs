namespace UZonMail.DB.SQL.Base
{
    /// <summary>
    /// 仅包含用户 ID 的 SqlId 类
    /// </summary>
    public class BaseUserId : SqlId
    {
        /// <summary>
        /// 用户的 id
        /// </summary>
        public long UserId { get; set; }
    }
}
