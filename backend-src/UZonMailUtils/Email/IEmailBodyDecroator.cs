using System.Threading.Tasks;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Utils.Email
{
    /// <summary>
    /// 邮件正文修饰器
    /// 使用私有变量时，注意实例是多用户共用的
    /// </summary>
    public interface IEmailBodyDecroator : ITransientService<IEmailBodyDecroator>
    {
        /// <summary>
        /// 开始进行装饰
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="originBody"></param>
        /// <returns></returns>
        Task<string> StartDecorating(IEmailDecoratorParams decoratorParams, string originBody);
    }
}
