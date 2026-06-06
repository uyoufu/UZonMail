using log4net;
using MailKit.Net.Proxy;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// 代理类
    /// 格式为：username:password@host:port
    /// </summary>
    [Keyless]
    public class ProxyInfo
    {
        /// <summary>
        /// 协议
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        public ProxyInfo(string proxyString)
        {
            // 将字符串转换为代理
            Uri uri = new(proxyString);
            Host = uri.Host;
            Port = uri.Port;
            Schema = uri.Scheme;

            var userInfos = uri.UserInfo.Split(':');
            if (userInfos.Length > 0)
            {
                Username = userInfos[0];
            }
            if (userInfos.Length > 1)
            {
                Password = userInfos[1];
            }
        }

        /// <summary>
        /// 与字符串的转换
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Schema}://{Username}:{Password}@{Host}:{Port}";
        }

        private ProxyClient _proxyClient;

        /// <summary>
        /// 生成代理客户端
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public ProxyClient? GetProxyClient(ILog logger)
        {
            if (_proxyClient != null) return _proxyClient;

            NetworkCredential networkCredential = new(Username, Password);
            switch (Schema.ToLower())
            {
                case "socks5":
                    _proxyClient = new Socks5Client(Host, Port, networkCredential);
                    break;
                case "http":
                    return new HttpProxyClient(Host, Port, networkCredential);
                case "https":
                    return new HttpsProxyClient(Host, Port, networkCredential);
                case "socks4":
                    return new Socks4Client(Host, Port, networkCredential);
                case "socks4a":
                    return new Socks4aClient(Host, Port, networkCredential);
                default:
                    logger.Error($"不支持的代理协议{Schema}");
                    break;
            }
            return _proxyClient;
        }

        /// <summary>
        /// 尝试解析代理字符串
        /// </summary>
        /// <param name="proxyString"></param>
        /// <param name="proxyInfo"></param>
        /// <returns></returns>
        public static bool CanParse(string proxyString)
        {
            return Uri.TryCreate(proxyString, UriKind.RelativeOrAbsolute, out _);
        }
    }
}
