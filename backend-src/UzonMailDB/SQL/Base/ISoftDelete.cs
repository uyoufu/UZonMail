namespace UZonMail.DB.SQL.Base
{
    /// <summary>
    /// soft delete interface
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indicates whether the entity is deleted
        /// This will be used for soft delete functionality in global query filters
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
