using UZonMail.Utils.Email;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Emails
{
    public class EmailBodyDecorateService(IServiceProvider serviceProvider) : IScopedService
    {
        /// <summary>
        /// 执行所有的装饰器，然后返回
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<string> Decorate(IEmailDecoratorParams decoratorParams, string body)
        {
            var decorators = serviceProvider.GetServices<IEmailBodyDecroator>();
            foreach (var decorator in decorators)
            {
                body = await decorator.StartDecorating(decoratorParams, body);
            }
            return body;
        }
    }
}
