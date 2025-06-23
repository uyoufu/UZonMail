using Microsoft.Extensions.Configuration;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMailDesktop.Pages;
using UZonMailDesktop.Pages.Tray;
using UZonMailDesktop.Utils;

namespace UZonMailDesktop
{
    internal class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void OnStart()
        {
            // 获取命令行参数
            string[] args = Environment.GetCommandLineArgs();
            base.OnStart();
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            // 添加配置
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{env}.json", optional: true)
               .Build();
            builder.Bind<IConfiguration>().ToInstance(configuration);

            // Bootstrapper 默认自动绑定
            // builder.Autobind();

            // 注册服务
            builder.Bind<BackEndService>().ToSelf().InSingletonScope();
            builder.Bind<SystemTrayWinfom>().ToSelf().InSingletonScope();
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts            
        }
    }
}
