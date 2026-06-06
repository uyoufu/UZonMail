using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.EmailDecorator.Interfaces
{
    /// <summary>
    /// 变量解析器, 继承于 IContentDecroator
    /// </summary>
    public interface IVariableResolver : ITransientService<IVariableResolver>, IContentDecroator { }
}
