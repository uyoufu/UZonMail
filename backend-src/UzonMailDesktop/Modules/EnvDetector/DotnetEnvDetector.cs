using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMailDesktop.Modules.EnvDetector
{
    internal class DotnetEnvDetector(string versionRequired): IRuntimeEnvDetector
    {
        public string Name => $"ASP.NET Core {versionRequired}";

        public string FailedMessage => $"{Name} 运行时环境缺失";

        public string RedirectUrl => $"https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-aspnetcore-{versionRequired}-windows-hosting-bundle-installer";

        public Lazy<bool> IsInstalled => new(()=> DetectEnv());

        private bool DetectEnv()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string[] outputs = output.Split('\n');

            if (outputs.Any(x=>x.StartsWith("Microsoft.AspNetCore.App 9.")))
                return true;

            // 若没有时，判断路径
            string dotnetDir = "C:\\Program Files\\dotnet\\shared\\Microsoft.AspNetCore.App";
            // 获取子文件夹名
            var dirs = Directory.GetDirectories(dotnetDir);
            if (dirs.Length == 0)
                return false;

            // 转成版本号
            var versions = dirs.Select(x => Path.GetFileName(x)).Select(x=> new Version(x));
            var needVersion = new Version(versionRequired);
            if (versions.Any(x => x >= needVersion)) return true;

            return false;
        }
    }
}
