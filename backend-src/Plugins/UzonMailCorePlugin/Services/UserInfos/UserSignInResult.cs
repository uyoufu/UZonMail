using UZonMail.DB.SQL.Core.Organization;

namespace UZonMail.CorePlugin.Services.UserInfos
{
    /// <summary>
    /// 用户登陆结果
    /// </summary>
    public class UserSignInResult
    {
        public string Token { get; set; }
        public List<string> Access { get; set; }
        public User UserInfo { get; set; }

        /// <summary>
        /// 已安装的插件
        /// </summary>
        public List<string> InstalledPlugins { get; set; }
    }
}
