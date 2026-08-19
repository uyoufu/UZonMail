using UzonMail.Utils.Web.Configs;

namespace UzonMail.CorePlugin.Config.SubConfigs
{
    public class UserOptions : IAppOptions
    {
        /// <summary>
        /// 管理员用户设置
        /// </summary>
        public AdminUser AdminUser { get; set; } = new();

        /// <summary>
        /// 默认用户密码
        /// </summary>
        public string DefaultPassword { get; set; } = string.Empty;
    }
}
