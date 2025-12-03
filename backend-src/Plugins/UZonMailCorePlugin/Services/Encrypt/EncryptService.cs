using UZonMail.Core.Services.Encrypt.Models;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.Configs;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Encrypt
{
    /// <summary>
    /// 加密相关的服务
    /// </summary>
    public class EncryptService(IConfiguration config)
        : ISingletonService<IEncryptService>,
            IEncryptService
    {
        private EncryptParams? _encrypParams;

        public EncryptParams GetEncrypParams()
        {
            if (_encrypParams != null)
                return _encrypParams;

            _encrypParams = config.GetConfig<EncryptParams>();
            return _encrypParams;
        }

        /// <summary>
        /// check whether the password is masked
        /// if the password is "******", it means it is masked
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPasswordMask(string password)
        {
            return password == GetPasswordMask();
        }

        public string GetPasswordMask()
        {
            return "******";
        }

        /// <summary>
        /// 使用盐加密密码
        /// </summary>
        /// <param name="hashedPwd">前端加密后的密码，由于密码传入后端时，已经是二次加密过的</param>
        /// <param name="salt">盐值，增加密码的破解验证</param>
        /// <returns></returns>
        public string HashPassword(string hashedPwd, string salt)
        {
            return (hashedPwd + salt).Sha256();
        }

        /// <summary>
        /// 使用对称加密方式加密密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncrytPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return password;

            var encryptParams = GetEncrypParams();
            return password.AES(encryptParams.Key, encryptParams.Iv);
        }

        /// <summary>
        /// 解密发件箱的密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string DecryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return password;

            var encryptParams = GetEncrypParams();
            return password.DeAES(encryptParams.Key, encryptParams.Iv);
        }
    }
}
