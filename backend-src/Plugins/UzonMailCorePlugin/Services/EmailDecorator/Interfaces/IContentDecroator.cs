using System.Threading.Tasks;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.EmailDecorator.Interfaces
{
    /// <summary>
    /// 邮件正文修饰器
    /// 使用私有变量时，注意实例是多用户共用的
    /// </summary>
    public interface IContentDecroator : ITransientService<IContentDecroator>
    {
        /// <summary>
        /// 执行顺序号
        /// 越小越先执行
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 开始进行装饰
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="originContent"></param>
        /// <returns></returns>
        Task<string> StartDecorating(IContentDecoratorParams decoratorParams, string originContent);
    }
}
