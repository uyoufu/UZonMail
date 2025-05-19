using UZonMail.Core.Services.EmailDecorator.Interfaces;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.EmailDecorator
{
    /// <summary>
    /// 邮件内容装饰器
    /// </summary>
    /// <param name="serviceProvider"></param>
    public class EmailContentDecorateService(IServiceProvider serviceProvider) : IScopedService
    {
        /// <summary>
        /// 执行所有的装饰器，然后返回
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<string> Decorate(IContentDecoratorParams decoratorParams, string content)
        {
            var decorators = serviceProvider.GetServices<IContentDecroator>()
                .OrderBy(x => x.Order);
            foreach (var decorator in decorators)
            {
                content = await decorator.StartDecorating(decoratorParams, content);
            }
            return content;
        }
    }
}
