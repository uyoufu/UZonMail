using Quartz;
using UZonMail.DB.SQL;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Jobs;
using UZonMail.Core.Database.Startup;
using UZonMail.Core.Database.Updater;
using UZonMail.Core.Config;
using UZonMail.Utils.Database.Initializer;
using UZonMail.Utils.Web.Service;
using UZonMailService.Services.PostStartup;

namespace UZonMail.Core.Services.HostedServices
{
    /// <summary>
    /// 程序启动时，开始中断的发件任务
    /// </summary>
    public class CoreHostedService(IServiceProvider serviceProvider) : IHostedServiceStart, IScopedService<IHostedServiceStart>
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 数据库启动设置
            var dbStartup = serviceProvider.GetRequiredService<DatabaseSetup>();
            await dbStartup.Start();

            // 数据升级
            var dataUpdater = serviceProvider.GetRequiredService<DatabaseUpdateService>();
            await dataUpdater.Update();
        }
    }
}
