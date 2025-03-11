using Uamazing.Utils.Plugin;
using UZonMail.Core.Config;
using UZonMail.Utils.Web;
using UZonMail.Core.SignalRHubs;
using UZonMail.Utils.Extensions;

namespace UZonMail.Core
{
    public class PluginSetup: IPlugin
    {
        public void UseServices(WebApplicationBuilder webApplicationBuilder)
        {
            var services = webApplicationBuilder.Services;
            // 绑定配置
            services.Configure<AppConfig>(webApplicationBuilder.Configuration);
            // 批量注册服务
            services.AddServices();
        }

        public void UseApp(WebApplication webApplication)
        {
            // SignalR 配置
            webApplication.MapHub<UzonMailHub>($"/hubs/{nameof(UzonMailHub).ToCamelCase()}");
        }
    }
}
