using System;
using System.Net.NetworkInformation;

namespace UZonMail.Utils.Web.Token
{
    /// <summary>
    /// 创建 token 的参数
    /// </summary>
    public class TokenParams
    {
        /// <summary>
        /// 过期时间
        /// ms
        /// </summary>
        public long Expire { get; set; } = 24 * 60 * 60 * 1000; // 默认 24 小时

        /// <summary>
        /// 过期时间，优先级最高
        /// 程序最终使用该值作为过期时间
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// token 的密钥
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// 签发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 接收者
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        private string _machineSecret = string.Empty;
        public string UniqueSecret
        {
            get
            {
                if (string.IsNullOrEmpty(_machineSecret))
                {
                    var mac = GetMacAddress();
                    _machineSecret = $"{mac}-{Secret}";
                }

                return _machineSecret;
            }
        }

        /// <summary>
        /// 获取物理网卡的 MAC 地址
        /// </summary>
        /// <returns></returns>
        private static string GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 只考虑物理网络接口
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress().ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取 TokenParams 的克隆
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public TokenParams Clone()
        {
            return new TokenParams
            {
                Expire = Expire,
                Secret = Secret,
                Issuer = Issuer,
                Audience = Audience,
                _machineSecret = _machineSecret
            };
        }
    }
}
