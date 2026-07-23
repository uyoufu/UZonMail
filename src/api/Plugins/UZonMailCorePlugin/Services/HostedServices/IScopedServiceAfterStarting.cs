using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.HostedServices
{
    /// <summary>
    /// 服务启动时执行
    /// </summary>
    public interface IScopedServiceAfterStarting : IScopedService<IScopedServiceAfterStarting>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        int Order { get; }

        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
