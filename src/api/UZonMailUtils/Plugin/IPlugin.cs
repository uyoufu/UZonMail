using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace UzonMail.Utils.Plugin
{
    /// <summary>
    /// 插件
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 配置优先级，数值越小越先执行；插件依赖顺序优先于该值
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
