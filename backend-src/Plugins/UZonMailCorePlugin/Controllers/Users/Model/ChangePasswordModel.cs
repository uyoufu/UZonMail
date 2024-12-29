namespace UZonMail.Core.Controllers.Users.Model
{
    public class ChangePasswordModel
    {
        /// <summary>
        /// 旧密码
        /// 前端通过 sha256 加密过
        /// </summary>
        public string OldPassword { get; set;}

        /// <summary>
        /// 新密码
        /// 前端通过 sha256 加密过
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// 原密码的 Smtp 解密密钥，用于将原密码解密
        /// </summary>
        public SmtpPasswordSecretKeys OldSmtpPasswordSecretKeys { get; set; }

        /// <summary>
        /// 新密码的 Smtp 加密密钥，用于将新密码加密
        /// </summary>
        public SmtpPasswordSecretKeys NewSmtpPasswordSecretKeys { get; set; }
    }
}
