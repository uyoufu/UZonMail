namespace UZonMail.DB.SQL.Base
{
    /// <summary>
    /// 带有组织 Id 的基类
    /// </summary>
    public class OrgId : SqlId
    {
        /// <summary>
        /// 带有组织 Id 的基类
        /// </summary>
        public long OrganizationId { get; set; }
    }
}
