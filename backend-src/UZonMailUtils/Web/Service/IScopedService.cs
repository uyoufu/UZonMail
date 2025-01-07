using System;
using System.Collections.Generic;
using System.Text;

namespace UZonMail.Utils.Web.Service
{
    /// <summary>
    /// 生命周期为请求周期内的服务
    /// </summary>
    public interface IScopedService : IService
    {
    }

    public interface IScopedService<T> : IScopedService
    {

    }
}
