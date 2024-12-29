namespace UZonMail.DB.SQL.NoEntity
{
    /// <summary>
    /// 仅有 Name 的数据引用
    /// </summary>
    public interface IDataRefName
    {
        long Id { get; set; }
        string Name { get; set; }
    }
}
