using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.CorePlugin.Config.SubConfigs
{
    public class UserConfig
    {
        public string CachePath { get; set; }

        /// <summary>
        /// 管理员用户设置
        /// </summary>
        public AdminUser AdminUser { get; set; }

        /// <summary>
        /// 默认用户密码
        /// </summary>
        public string DefaultPassword { get; set; }
    }
}
