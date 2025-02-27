namespace UZonMail.DB.SQL.Base
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
