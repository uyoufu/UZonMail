using System;
using System.Threading.Tasks;
using MimeKit;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.EmailDecorator.Interfaces
{
    /// <summary>
    /// 邮件正文修饰器
    /// 使用私有变量时，注意实例是多用户共用的
    /// </summary>
    public interface IMimeMessageDecroator : ITransientService<IMimeMessageDecroator>
    {
        /// <summary>
        /// 开始进行装饰
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="originBody"></param>
        /// <returns></returns>
        Task<MimeMessage> StartDecorating(
            IContentDecoratorParams decoratorParams,
            MimeMessage mimeMessage
        );
    }
}
