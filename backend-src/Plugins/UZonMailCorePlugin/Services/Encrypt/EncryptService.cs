using System.Collections.Concurrent;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Encrypt
{
    /// <summary>
    /// 加密相关的服务
    /// </summary>
    public class EncryptService : ISingletonService
    {
        private static readonly ConcurrentDictionary<
            long,
            SmtpPasswordSecretKeys
        > _userOutboxSecretKeys = new();

        /// <summary>
        /// 更新用户的发件箱密码密钥
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="secretKeys"></param>
        public void UpdateUserEncryptKeys(long userId, SmtpPasswordSecretKeys secretKeys)
        {
            if (_userOutboxSecretKeys.ContainsKey(userId))
            {
                _userOutboxSecretKeys[userId] = secretKeys;
            }
            else
                _userOutboxSecretKeys.TryAdd(userId, secretKeys);
        }

        /// <summary>
        /// 加密用户密码
        /// 进行二次 hash 计算
        /// </summary>
        /// <param name="password">非加密过的密码</param>
        /// <returns></returns>
        public string EncryptUserPassword(string password)
        {
            return password.Sha256(1);
        }

        /// <summary>
        /// 加密发件箱的密码
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string EncryptOutboxSecret(long userId, string secret)
        {
            if (!_userOutboxSecretKeys.TryGetValue(userId, out var keys))
                throw new KnownException("用户的 SMTP 密码密钥未设置, 请重新登录");

            return secret.AES(keys.Key, keys.Iv);
        }

        /// <summary>
        /// 解密发件箱的密码
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string DecryptOutboxSecret(long userId, string secret)
        {
            if (!_userOutboxSecretKeys.TryGetValue(userId, out var keys))
                throw new KnownException("用户的 SMTP 密码密钥未设置, 请重新登录");

            return secret.DeAES(keys.Key, keys.Iv);
        }
    }
}
