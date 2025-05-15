using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMailDesktop.Pages.Conductors
{
    /// <summary>
    /// 只允许一个活动项的 Conductor
    /// </summary>
    internal class RouterConductor(IContainer ioc) : Conductor<RouteScreen>.StackNavigation
    {
        /// <summary>
        /// 激活一个路由显示
        /// </summary>
        /// <typeparam name="T">ViewModel类型默认注册为 Transient，若需要重复使用，请手动注册为单例</typeparam>
        public void Active<T>() where T : RouteScreen
        {
            var screen = ioc.Get<T>();
            screen.SetConductor(this);

            // 激活一个路由显示
            ActivateItem(screen);
        }
    }
}
