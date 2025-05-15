using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMailDesktop.Helpers;

namespace UZonMailDesktop.Modules.EnvDetector
{
    internal class Webview2EnvDetector : IRuntimeEnvDetector
    {
        public string Name => "WebView2";

        public string FailedMessage => "Webview2 环境环境缺失";

        public string RedirectUrl => "https://developer.microsoft.com/zh-cn/microsoft-edge/webview2?form=MA13LH#download";

        public Lazy<bool> IsInstalled => new(() => DetectEnv());

        private bool DetectEnv()
        {
            // 验证 webview2 环境
            return Webview2Helper.HasWebView2();
        }
    }
}
