using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Core.Config.SubConfigs
{
    public class UserConfig
    {
        /// <summary>
        /// 管理员用户设置
        /// </summary>
        public AdminUserConfig AdminUser { get; set; }

        /// <summary>
        /// 默认用户密码
        /// </summary>
        public string DefaultPassword { get; set; }
    }
}
