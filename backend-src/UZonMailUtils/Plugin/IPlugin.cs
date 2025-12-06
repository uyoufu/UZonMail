using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace UZonMail.Utils.Plugin
{
    /// <summary>
    /// Plugiin 的一些配置项
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 配置插件
        /// Warning: 该方法可能会被多次调用，请确保幂等性
        /// </summary>
        void ConfigureServices(IHostApplicationBuilder hostBuilder);

        /// <summary>
        /// 配置应用程序
        /// Warning: 该方法可能会被多次调用，请确保幂等性
        /// </summary>
        /// <param name="app"></param>
        void ConfigureApp(IApplicationBuilder app);
    }
}
