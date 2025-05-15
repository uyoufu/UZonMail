using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMailDesktop.Modules.EnvDetector
{
    internal interface IRuntimeEnvDetector
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get;}

        /// <summary>
        /// 环境是否已安装
        /// </summary>
        Lazy<bool> IsInstalled { get; }

        /// <summary>
        /// 失败后显示的消息
        /// </summary>
        string FailedMessage { get; }

        /// <summary>
        /// 跳转链接
        /// </summary>
        string RedirectUrl { get; }
    }
}
