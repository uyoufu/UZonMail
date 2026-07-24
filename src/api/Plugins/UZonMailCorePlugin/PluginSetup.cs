using UzonMail.CorePlugin.Config;
using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Reading;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Plugin;
using UzonMail.Utils.Web;

namespace UzonMail.CorePlugin
{
    public class PluginSetup : IPlugin
    {
        public int Priority => 0;

        public void ConfigureServices(IHostApplicationBuilder hostBuilder)
        {
            var services = hostBuilder.Services;
            // 绑定配置
            services.Configure<AppConfig>(hostBuilder.Configuration);
            services.Configure<SendItemReaderOptions>(
                hostBuilder.Configuration.GetSection(SendItemReaderOptions.SectionName)
            );
            services.Configure<SendingQuotaOptions>(
                hostBuilder.Configuration.GetSection(SendingQuotaOptions.SectionName)
            );
            services.AddSingleton(TimeProvider.System);
            // 批量注册服务
            services.AddServices();

            // SendingTasksManager 同时实现两个契约，必须共享同一个调度状态。
            services.AddSingleton<SendingTasksManager>();
            services.AddSingleton<ISendingTasksManager>(provider =>
                provider.GetRequiredService<SendingTasksManager>()
            );
            services.AddSingleton<ISendingWorkerCoordinator>(provider =>
                provider.GetRequiredService<SendingTasksManager>()
            );
        }

        public void ConfigureApp(IApplicationBuilder app)
        {
            // SignalR 配置
            (app as WebApplication)!.MapHub<UzonMailHub>(
                $"/hubs/{nameof(UzonMailHub).ToCamelCase()}"
            );
        }
    }
}
