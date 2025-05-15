using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMailDesktop.Modules.EnvDetector;
using UZonMailDesktop.Pages.Conductors;

namespace UZonMailDesktop.Pages.MissingEnv
{
    internal class MissingEnvViewModel(IRuntimeEnvDetector env) : RouteScreen
    {
        public string Name => env.Name;

        public string FailedMessage => env.FailedMessage;

        public string RedirectUrl => env.RedirectUrl;

        public void DownloadEnv()
        {
            var psi = new ProcessStartInfo
            {
                FileName = RedirectUrl,
                UseShellExecute = true // 关键点：让系统用默认方式打开
            };
            Process.Start(psi);
        }
    }
}
