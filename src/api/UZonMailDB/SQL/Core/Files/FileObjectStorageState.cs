namespace UzonMail.DB.SQL.Core.Files
{
    /// <summary>
    /// 文件对象在数据库与磁盘之间的持久化状态。
    /// </summary>
    public enum FileObjectStorageState
    {
        Pending = 0,
        Ready = 1,
    }
}
