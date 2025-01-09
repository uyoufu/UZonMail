using MimeKit;
using UZonMail.Utils.Email;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Emails
{
    /// <summary>
    /// 邮件消息体装饰器
    /// </summary>
    /// <param name="serviceProvider"></param>
    public class MimeMessageDecorateService(IServiceProvider serviceProvider) : IScopedService
    {
        /// <summary>
        /// 执行所有的装饰器，然后返回
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public async Task<MimeMessage> Decorate(EmailDecoratorParams decoratorParams, MimeMessage mimeMessage)
        {
            var decorators = serviceProvider.GetServices<IMimeMessageDecroator>();
            foreach (var decorator in decorators)
            {
                mimeMessage = await decorator.StartDecorating(decoratorParams, mimeMessage);
            }

            return mimeMessage;
        }
    }
}
