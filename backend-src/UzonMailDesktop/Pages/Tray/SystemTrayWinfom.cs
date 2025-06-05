using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using UZonMailDesktop.Utils;

namespace UZonMailDesktop.Pages.Tray
{
    /// <summary>
    /// windows 托盘功能
    /// 比 wpf 版本快
    /// </summary>
    public class SystemTrayWinfom(BackEndService backEndService) : ISystemTrayIcon
    {
        private NotifyIcon _notifyIcon = null;
        private Window _window;

        /// <summary>
        /// 加载托盘
        /// </summary>
        /// <param name="window"></param>
        public void Start(Window window)
        {
            _window = window;
            _window.Closing += Window_Closing;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("退出", null, Exit_Click);//添加退出菜单项

            //设置托盘的各个属性
            _notifyIcon = new NotifyIcon
            {
                BalloonTipText = $"{window.Title} 运行中...",//托盘气泡显示内容
                Text = window.Title,
                Visible = true,//托盘按钮是否可见
                Icon = new Icon("./Resource/uzon-mail.ico"), //托盘中显示的图标
                ContextMenuStrip = contextMenu,
            };

            //鼠标点击事件
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        #region 私有

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _window.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        // 托盘图标鼠标单击事件
        private void NotifyIcon_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            //鼠标左键，实现窗体最小化隐藏或显示窗体
            if (e.Button == MouseButtons.Left)
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
            }
        }

        // 退出选项
        private void Exit_Click(object? sender, EventArgs e)
        {
            if (System.Windows.MessageBox.Show("即将退出宇正群邮, 是否继续?", "温馨提醒", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                // 关闭后台程序
                backEndService.CloseExist();

                // 退出应用
                Environment.Exit(0);
            }
        }
        #endregion
    }
}
