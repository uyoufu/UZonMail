using Microsoft.AspNetCore.SignalR;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Core.Services.Encrypt;

namespace UZonMail.Core.SignalRHubs
{
    /// <summary>
    /// 客户端调用向服务器发送用户的加密密钥
    /// </summary>
    public partial class UzonMailHub
    {
        /// <summary>
        /// 设置用户加密密钥
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public Task SetUserEncryptKeys(long userId, string key, string iv)
        {
            encryptService.UpdateUserEncryptKeys(userId, new SmtpPasswordSecretKeys()
            {
                Key = key,
                Iv = iv
            });
            return Task.CompletedTask;
        }
    }
}
