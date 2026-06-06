using System.Security.Cryptography;
using System.Text;

namespace UZonMail.Utils.Extensions
{
    public static class RSAExtensions
    {
        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string FromRSA(this byte[] encryptedData, string privateKey, int keySizeInBit = 2048)
        {
            var rsa = RSA.Create(keySizeInBit);
            rsa.ImportFromPem(privateKey);
            byte[] decryptedDataBytes = rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedDataBytes);
        }
    }
}
