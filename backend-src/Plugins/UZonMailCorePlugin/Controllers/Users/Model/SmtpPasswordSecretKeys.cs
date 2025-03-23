using UZonMail.Utils.Web.Exceptions;

namespace UZonMail.Core.Controllers.Users.Model
{
    /// <summary>
    /// SmtpPasswordSecretKeys
    /// </summary>
    public class SmtpPasswordSecretKeys
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Iv { get; set; }

        /// <summary>
        /// 从字符串列表中获取密钥
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static SmtpPasswordSecretKeys Create(List<string> strings)
        {
            if(strings.Count<2)
            {
               throw new KnownException("密钥不完整");
            }

            return new SmtpPasswordSecretKeys
            {
                Key = strings[0],
                Iv = strings[1]
            };
        }
    }
}
