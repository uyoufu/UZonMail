using Microsoft.Extensions.Configuration;
using UzonMail.Utils.Web.Service;

namespace UzonMail.Utils.Web.Configs
{
    /// <summary>
    /// 设置类配置
    /// 获取方式为注入 AppOptions<T> 类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    public class AppOptions<T>(IConfiguration configuration)
        : ISingletonService<IAppOptions<T>>,
            IAppOptions<T>
        where T : class, new()
    {
        private readonly T _value = configuration.GetConfig<T>();
        public T Value => _value;
    }
}
