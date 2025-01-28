using System.Net;

namespace UZonMail.Utils.Http
{
    public class UriUserInfo
    {
        public UriUserInfo(string userInfo)
        {
            if (string.IsNullOrEmpty(userInfo))
            {
                return;
            }

            var parts = userInfo.Split(':');
            if (parts.Length == 2)
            {
                UserName = parts[0];
                Password = parts[1];
                return;
            }

            UserName = userInfo;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; } = null;

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; } = null;

        public NetworkCredential GetCredential()
        {
            return new NetworkCredential(UserName, Password);
        }
    }
}
