using UzonMail.DB.SQL.Core.Organization;

namespace UzonMail.CorePlugin.Services.UserInfos
{
    /// <summary>
    /// 用户登陆结果
    /// </summary>
    public class UserSignInResult
    {
        public required string Token { get; set; }
        public required List<string> Access { get; set; }
        public required User UserInfo { get; set; }

        /// <summary>
        /// 已安装的插件
        /// </summary>
        public required List<string> InstalledPlugins { get; set; }
    }
}
