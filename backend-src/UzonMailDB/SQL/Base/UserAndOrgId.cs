namespace UZonMail.DB.SQL.Base
{
    /// <summary>
    /// 用户和组织 id
    /// </summary>
    public class UserAndOrgId : OrgId
    {
        /// <summary>
        /// 用户的 id
        /// </summary>
        public long UserId { get; set; }
    }
}
