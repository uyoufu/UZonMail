
using System.Runtime.Intrinsics.X86;

namespace UZonMailService.Services.PostStartup
{
    /// <summary>
    /// 服务启动后的后台服务
    /// </summary>
    public class HostedServiceStartup(IServiceScopeFactory ssf) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = ssf.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var postServices = serviceProvider.GetServices<IHostedServiceStart>()
                .OrderBy(x => x.Order);
            foreach (var postService in postServices)
            {
                await postService.ExecuteAsync(stoppingToken);
            }
        }
    }
}
