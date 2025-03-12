namespace UZonMail.DB.SQL.Core.Organization
{
    /// <summary>
    /// 子用户的策略
    /// </summary>
    public enum SubUserStrategy
    {
        /// <summary>
        /// 跟随主账号
        /// </summary>
        FollowMaster,

        /// <summary>
        /// 独立
        /// </summary>
        Independent,
    }
}
