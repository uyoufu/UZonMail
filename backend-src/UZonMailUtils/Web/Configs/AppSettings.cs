using Microsoft.Extensions.Configuration;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Utils.Web.Configs
{
    /// <summary>
    /// 设置类配置
    /// 获取方式为注入 AppSettings<T> 类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    public class AppSettings<T>(IConfiguration configuration) : ITransientService
        where T : class, new()
    {
        public T Value { get; set; } = configuration.GetConfig<T>();
    }
}
