namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    /// <summary>
    /// 用户发件资源释放：发件箱、发件组等
    /// </summary>
    public interface IUserResourceDispose
    {
        void Dispose(long userId);
    }
}
