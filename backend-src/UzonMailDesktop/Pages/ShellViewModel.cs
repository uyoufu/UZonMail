using Microsoft.Extensions.Configuration;
using Panuon.WPF.UI;
using StyletIoC;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using UZonMailDesktop.Modules.EnvDetector;
using UZonMailDesktop.Pages.Conductors;
using UZonMailDesktop.Pages.MissingEnv;
using UZonMailDesktop.Pages.Tray;
using UZonMailDesktop.Pages.Webview;
using UZonMailDesktop.Utils;
using MessageBox = System.Windows.MessageBox;

namespace UZonMailDesktop.Pages
{
    internal class ShellViewModel(IContainer ioc, IConfiguration configuration, BackEndService backEndService, SystemTrayWinfom systemTray) : RouterConductor(ioc)
    {
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        private Visibility _visibility;

        /// <summary>
        /// 显示状态
        /// </summary>
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                NotifyOfPropertyChange(() => Visibility);
            }
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            // 验证是否已经启动了
            var currentProcess = Process.GetCurrentProcess();
            var desktopProcess = Process.GetProcesses().Where(x => x.ProcessName == "UZonMailDesktop" && x.Id != currentProcess.Id).FirstOrDefault();
            if (desktopProcess != null)
            {
                // 提示已经启动，直接退出
                MessageBoxX.Show("不能重复运行", "温馨提示");
                // 打开原来的程序
                Environment.Exit(0);
            }

            if (!VerifyEnv()) return;

            // 启动后台服务
            backEndService.Start();

            // 获取当前文件的版本号
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            Title = $"宇正群邮 - {version}";

            // 跳转到主页面
            Active<UzonViewModel>();

            // 显示托盘图标
            ShowTrayIcon();
        }

        protected override void OnClose()
        {
            // 关闭后台服务
            backEndService.CloseExist();
            base.OnClose();
        }

        private bool VerifyEnv()
        {
            // 验证环境
            // 验证 dotnet
            var requiredVersion = configuration.GetSection("DotNETRequired").Value;
            if (string.IsNullOrEmpty(requiredVersion))
            {
                // 如果没有配置，使用默认值
                requiredVersion = "9.0.5";
            }
            var envDetectors = new List<IRuntimeEnvDetector>()
            {
                new DotnetEnvDetector(requiredVersion),
                new Webview2EnvDetector()
            };
            var missingEnv = envDetectors.Where(x => !x.IsInstalled.Value).FirstOrDefault();
            if (missingEnv != null)
            {
                // 如果没有安装，跳转到缺失环境页面
                ActivateItem(new MissingEnvViewModel(missingEnv));
                return false;
            }

            return true;
        }

        private void ShowTrayIcon()
        {
            systemTray.Start(View as Window);
        }
    }
}
