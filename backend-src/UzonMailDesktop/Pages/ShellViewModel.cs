using Microsoft.Extensions.Configuration;
using StyletIoC;
using System.Reflection;
using System.Windows;
using UZonMailDesktop.Modules.EnvDetector;
using UZonMailDesktop.Pages.Conductors;
using UZonMailDesktop.Pages.MissingEnv;
using UZonMailDesktop.Pages.Tray;
using UZonMailDesktop.Pages.Webview;
using UZonMailDesktop.Utils;

namespace UZonMailDesktop.Pages
{
    internal class ShellViewModel(IContainer ioc, IConfiguration configuration, BackEndService backEndService) : RouterConductor(ioc)
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

        private WindowsSystemTray? _notifyIcon;
        private void ShowTrayIcon()
        {
            _notifyIcon = new WindowsSystemTray(View as Window);
        }
    }
}
