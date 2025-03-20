using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.HostedServices
{
    /// <summary>
    /// 服务启动时执行
    /// </summary>
    public interface IHostedServiceStart : IScopedService<IHostedServiceStart>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        int Order { get; }

        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
