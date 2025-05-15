using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen = Stylet.Screen;

namespace UZonMailDesktop.Pages.Conductors
{
    /// <summary>
    /// 具有路由功能的页面
    /// </summary>
    internal abstract class RouteScreen : Screen
    {
        protected RouterConductor? Router { get; private set; }

        public void SetConductor(RouterConductor conductor)
        {
            // 设置路由管理
            this.Router = conductor;
        }
    }
}
