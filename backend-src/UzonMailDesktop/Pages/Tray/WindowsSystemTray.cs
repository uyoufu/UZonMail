using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace UZonMailDesktop.Pages.Tray
{
    /// <summary>
    /// windows 托盘功能
    /// </summary>
    internal class WindowsSystemTray
    {
        private TaskbarIcon _notifyIcon;
        private Window _window;

        /// <summary>
        /// 加载托盘
        /// </summary>
        /// <param name="window"></param>
        public WindowsSystemTray(Window window)
        {
            _window = window;
            _window.Closing += Window_Closing;

            var ctxMenu = new ContextMenu();
            var openCommand = new RelayCommand(() =>
            {
                _window.Visibility = Visibility.Visible;
                _window.WindowState = WindowState.Normal;
                _window.ShowInTaskbar = true;//使Form在任务栏上显示
                _window.Activate();
            });
            ctxMenu.Items.Add(new MenuItem()
            {
                Header = "打开",
                Command = openCommand
            });
            ctxMenu.Items.Add(new MenuItem()
            {
                Header = "退出",
                Command = new RelayCommand(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show(_window, "即将退出宇正群邮, 是否继续?", "温馨提醒", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        {
                            // 退出应用
                            Environment.Exit(0);
                        }
                    });
                })
            });

            var leftClickCommand = new RelayCommand(() =>
            {
                if (_window.Visibility == Visibility.Visible && _window.WindowState != WindowState.Minimized)
                {
                    _window.Visibility = Visibility.Hidden;
                    //解决最小化到任务栏可以强行关闭程序的问题。
                    _window.ShowInTaskbar = false;//使Form不在任务栏上显示
                }
                else
                {
                    _window.Visibility = Visibility.Visible;
                    _window.WindowState = WindowState.Normal;
                    _window.ShowInTaskbar = true;//使Form在任务栏上显示
                    _window.Activate();
                }
            });

            //设置托盘的各个属性
            _notifyIcon = new TaskbarIcon
            {
                Icon = new Icon("./Resource/uzon-mail.ico"),
                ContextMenu = ctxMenu,
                ToolTipText = "宇正群邮运行中...",
                Visibility = Visibility.Visible,
                LeftClickCommand = leftClickCommand
            };
        }

        #region 私有
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _window.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
        #endregion
    }
}
