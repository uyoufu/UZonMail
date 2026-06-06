using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using UZonMail.Utils.Results;

namespace UZonMail.Utils.Extensions
{
    /// <summary>
    /// MD5 验证器
    /// </summary>
    public static class EncryptExtensions
    {
        #region 单向加密
        /// <summary>
        /// 传入不同的算法计算 hash 值
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Hash(
            this string plainText,
            HashAlgorithm hashAlgorithm,
            string format = "x2"
        )
        {
            // 参考：https://www.cnblogs.com/qiufengke/p/5292621.html
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException($"{nameof(plainText)}为空");
            var bytHash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            var sb = new StringBuilder();
            foreach (var b in bytHash)
            {
                sb.Append(b.ToString(format));
            }
            var encryptText = sb.ToString();

            return encryptText;
        }

        /// <summary>
        /// 获得一个字符串的加密密文
        /// 此密文为单向加密，即不可逆(解密)密文
        /// </summary>
        /// <param name="plainText">待加密明文</param>
        /// <returns>已加密密文</returns>
        public static string MD5(this string plainText, int cycle = 0)
        {
            var hashAlgorithm = System.Security.Cryptography.MD5.Create();
            var hash = plainText.Hash(hashAlgorithm);
            if (cycle > 0)
            {
                return hash.MD5(cycle - 1);
            }
            return hash;
        }

        /// <summary>
        /// 判断明文与密文是否相符
        /// </summary>
        /// <param name="plainText">待检查的明文</param>
        /// <param name="encryptText">待检查的密文</param>
        /// <returns>bool</returns>
        public static bool EqualMd5(this string plainText, string encryptText)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(encryptText))
            {
                // throw new ArgumentNullException("明文或密文不存在");
                return false;
            }

            var md5Result = plainText.MD5();

            if (md5Result != encryptText)
            {
                // 明文与密文不匹配
                return false;
            }

            return true;
        }

        /// <summary>
        /// 计算字符串的 SHA256 值
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="extra">额外的循环的次数</param>
        /// <returns></returns>
        public static string Sha256(this string plainText, int extra = 0)
        {
            var hashAlgorithm = SHA256.Create();
            var hash = plainText.Hash(hashAlgorithm);

            if (extra > 0)
            {
                return hash.Sha256(extra - 1);
            }
            return hash;
        }
        #endregion

        #region 对称加密
        public static byte[] CryptoTransform(
            this string plainText,
            ICryptoTransform cryptoTransform
        )
        {
            // 参考：https://www.cnblogs.com/qiufengke/p/5292621.html
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException($"{nameof(plainText)}为空");
            var textBytes = Encoding.UTF8.GetBytes(plainText);
            return textBytes.CryptoTransform(cryptoTransform);
        }

        public static byte[] ReadBytesFromStream(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            return reader.ReadBytes((int)stream.Length);
        }

        /// <summary>
        /// 16 进制字符串转换为字节数组
        /// </summary>
        /// <param name="textBytes"></param>
        /// <param name="cryptoTransform"></param>
        /// <returns></returns>
        public static byte[] CryptoTransform(
            this byte[] textBytes,
            ICryptoTransform cryptoTransform
        )
        {
            using MemoryStream msDecrypt = new MemoryStream(textBytes);
            using CryptoStream csDecrypt = new CryptoStream(
                msDecrypt,
                cryptoTransform,
                CryptoStreamMode.Read
            );
            // 读取所有 bytes
            List<byte> bytes = new List<byte>();
            while (true)
            {
                var value = csDecrypt.ReadByte();
                if (value == -1)
                {
                    break;
                }

                bytes.Add((byte)value);
            }
            // 获取结果
            return bytes.ToArray();
        }

        /// <summary>
        /// 16 进制字符串转换为字节数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToByteArray(this string hex)
        {
            return
            [
                .. Enumerable
                    .Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            ];
        }

        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AES(this string plainText, string key, string iv)
        {
            // 将 key 取 hash 值, 避免长度不够
            var sha256Key = key.Sha256();
            var sha256Iv = iv.Sha256();
            var rgbKeyBytes = Encoding.UTF8.GetBytes(sha256Key)[..32];
            var rgbIvBytes = Encoding.UTF8.GetBytes(sha256Iv)[..16];

            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            var encryptor = aes.CreateEncryptor(rgbKeyBytes, rgbIvBytes);
            var bytes = plainText.CryptoTransform(encryptor);
            // 转换为 16 进制字符串
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="encryptText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DeAES(this string encryptText, string key, string iv)
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var sha256Key = key.Sha256();
            var sha256Iv = iv.Sha256();
            var rgbKeyBytes = Encoding.UTF8.GetBytes(sha256Key)[..32];
            var rgbIvBytes = Encoding.UTF8.GetBytes(sha256Iv)[..16];

            var decryptor = aes.CreateDecryptor(rgbKeyBytes, rgbIvBytes);
            var encryptArray = encryptText.HexToByteArray();
            var bytes = decryptor.TransformFinalBlock(encryptArray, 0, encryptArray.Length);
            // 转为 utf 字符串
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion
    }
}
