using System;
using System.Collections.Generic;
using System.Text;

namespace UZonMail.Utils.Web.Service
{
    /// <summary>
    /// 只要调用，就会创建一个新的实例的服务
    /// </summary>
    public interface ITransientService : IService
    {
    }

    public interface ITransientService<T> : ITransientService
    {

    }
}
