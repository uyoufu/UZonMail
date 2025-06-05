using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace UZonMailDesktop.Utils
{
    /// <summary>
    /// 后台服务
    /// </summary>
    public class BackEndService(IConfiguration configuration)
    {
        private readonly string ServiceName = "UzonMailService";

        private List<Process> GetUzonMailProcesses()
        {
            return Process.GetProcesses().Where(x => x.ProcessName == ServiceName).ToList();
        }

        public void Start()
        {
            // 关闭非当前目录中的后台服务
            CloseExist();

            // 判断是否有后台服务，若有，则不再启动
            var processes = GetUzonMailProcesses();
            if (processes.Count > 0)
            {
                return;
            }

            try
            {
                // 判断是否存在文件，若不存在，则进行提示
                var servicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service", "UZonMailService.exe");
                if (!File.Exists(servicePath))
                {
                    MessageBox.Show("后台服务缺失，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 启动服务
                var startInfo = new ProcessStartInfo
                {
                    FileName = servicePath,
                    WorkingDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service"),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(startInfo);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// 关闭非当前目录中的后台服务
        /// </summary>
        public void CloseIfNotSelf()
        {
            var processes = GetUzonMailProcesses();
            string servicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service");

            foreach (var process in processes)
            {
                string query = $"SELECT ProcessId,ExecutablePath FROM Win32_Process WHERE ProcessId = {process.Id}";
                using ManagementObjectSearcher searcher = new(query);
                ManagementObject? mo = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (mo == null)
                    continue;

                var executablePath = mo["ExecutablePath"]?.ToString();
                if (string.IsNullOrEmpty(executablePath))
                    continue;

                if (!executablePath.StartsWith(servicePath))
                {
                    // 非当前进程目录下的进程服务，关闭
                    process.Kill();
                }
            }
        }

        /// <summary>
        /// 关闭后台服务
        /// </summary>
        public void CloseAll()
        {
            // 查找进程名为 UZonMailService 的进程并杀死
            var processes = GetUzonMailProcesses();
            foreach (var item in processes)
            {
                item.Kill();
            }
        }

        /// <summary>
        /// 当窗口关闭时调用
        /// </summary>
        public void CloseExist()
        {
            // 获取配置
            var keepBackService = configuration.GetSection("KeepBackEndAliveWhenExit").Value;

            if(bool.TryParse(keepBackService, out bool keep) && keep)
            {
                CloseIfNotSelf();
            }
            else
            {
                CloseAll();
            }
        }
    }
}
