using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.DB.PostgreSql;
using UzonMail.DB.SQL;
using UzonMail.DB.SqLite;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Plugin;

namespace UzonMail.CorePlugin
{
    public class PluginSetup : IPlugin
    {
        public int Priority => 0;

        public void ConfigureServices(IHostApplicationBuilder hostBuilder)
        {
            var services = hostBuilder.Services;

            // 添加数据库上下文
            services.AddSqlContext<SqlContext, PostgreSqlContext, SqLiteContext>(
                hostBuilder.Configuration
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
