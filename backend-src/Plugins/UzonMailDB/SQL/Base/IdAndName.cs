namespace UZonMail.DB.SQL.Base
{
    /// <summary>
    /// Id 和名字
    /// </summary>
    public class IdAndName : SqlId
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }
}
