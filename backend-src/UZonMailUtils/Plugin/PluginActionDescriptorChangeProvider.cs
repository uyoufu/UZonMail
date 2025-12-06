using System.Threading;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace UZonMail.Utils.Plugin
{
    /// <summary>
    /// 目前采用的第一种方案
    /// 方案一: https://www.codetd.com/article/461093
    /// 方案二: https://www.cnblogs.com/artech/p/dynamic-controllers.html
    /// </summary>
    public class PluginActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        private PluginActionDescriptorChangeProvider() { }

        public static PluginActionDescriptorChangeProvider Instance { get; } =
            new PluginActionDescriptorChangeProvider();

        private CancellationTokenSource? _tokenSource;

        /// <summary>
        /// 获取变更信号
        /// 在 MapControllers 后调用
        /// </summary>
        /// <returns></returns>
        public IChangeToken GetChangeToken()
        {
            _tokenSource = new CancellationTokenSource();
            return new CancellationChangeToken(_tokenSource.Token);
        }

        /// <summary>
        /// 发送变更信号
        /// 在程序集第一次加载时，不需要调用此方法
        /// </summary>
        public void SignalChange()
        {
            _tokenSource?.Cancel();
        }
    }
}
