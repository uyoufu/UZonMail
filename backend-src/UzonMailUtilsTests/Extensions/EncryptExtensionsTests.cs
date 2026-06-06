using UZonMail.Utils.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UZonMail.Utils.Extensions.Tests
{
    [TestClass()]
    public class EncryptExtensionsTests
    {
        [DataRow("admin1234", "admin")]
        [TestMethod()]
        public void DeAESTest(string pasword, string userId)
        {
            // 转成 md5
            var userIdMd5 = userId.MD5();
            // 加密
            var encrypt = pasword.AES(userIdMd5, userIdMd5.Substring(0, 16));
            // 解密
            var decrypt = encrypt.DeAES(userIdMd5, userIdMd5.Substring(0, 16));
            // 判断是否相等
            Assert.IsTrue(decrypt == pasword);
            Assert.AreEqual("7e8835850f093879c8be909adecbd373", encrypt);
        }

        [DataRow("admin1234")]
        [TestMethod()]
        public void MD5Test(string plainValue)
        {
            var value = plainValue.MD5();
            Assert.AreEqual("c93ccd78b2076528346216b3b2f701e6", value);
        }
    }
}