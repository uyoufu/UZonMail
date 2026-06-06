using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Database.Initializers
{
    /// <summary>
    /// 数据库初始化器
    /// </summary>
    public interface IDbInitializer : IScopedService<IDbInitializer>
    {
        string Name { get; }
        Task ExecuteAsync();
    }
}
