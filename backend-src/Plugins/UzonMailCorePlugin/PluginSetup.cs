using UZonMail.CorePlugin.Config;
using UZonMail.CorePlugin.SignalRHubs;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Plugin;
using UZonMail.Utils.Web;

namespace UZonMail.CorePlugin
{
    public class PluginSetup : IPlugin
    {
        public int Priority => 0;

        public void ConfigureServices(IHostApplicationBuilder hostBuilder)
        {
            var services = hostBuilder.Services;
            // 绑定配置
            services.Configure<AppConfig>(hostBuilder.Configuration);
            // 批量注册服务
            services.AddServices();
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
