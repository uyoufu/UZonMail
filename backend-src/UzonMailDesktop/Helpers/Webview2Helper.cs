using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzonMailDesktop.Helpers
{
    public class Webview2Helper
    {
        /// <summary>
        /// 是否有 edge
        /// </summary>
        /// <returns></returns>
        public static bool HasWebView2()
        {
            var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            var installInfo = new InstallInfo(version);
            return !installInfo.InstallType.Equals(InstallType.NotInstalled);
        }

        public static bool HasWebView2InstalledByReg()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"))
            {
                if (key == null) return false;
                var versionStr = key.GetValue("pv");
                if (versionStr == null) return false;
                return !string.IsNullOrEmpty(versionStr.ToString());
            }
        }
    }

    public class InstallInfo
    {
        public InstallInfo(string version) => (Version) = (version);

        public string Version { get; }

        public InstallType InstallType
        {
            get
            {
                if(Version.Contains("dev"))return InstallType.EdgeChromiumDev;
                if (Version.Contains("beta")) return InstallType.EdgeChromiumBeta;
                if (Version.Contains("canary")) return InstallType.EdgeChromiumCanary;
                if (!string.IsNullOrEmpty(Version)) return InstallType.WebView2;
                return InstallType.NotInstalled;
            }           
        }
    }

    public enum InstallType
    {
        WebView2, 
        EdgeChromiumBeta, 
        EdgeChromiumCanary, 
        EdgeChromiumDev, 
        NotInstalled
    }
}
