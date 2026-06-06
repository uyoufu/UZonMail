namespace UZonMail.CorePlugin.Controllers.Users.Model
{
    public class ChangePasswordModel
    {
        /// <summary>
        /// 旧密码
        /// 前端通过 sha256 加密过
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// 新密码
        /// 前端通过 sha256 加密过
        /// </summary>
        public string NewPassword { get; set; }
    }
}
