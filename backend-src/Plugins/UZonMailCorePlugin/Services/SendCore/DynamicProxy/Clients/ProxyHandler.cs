using log4net;
using MailKit.Net.Proxy;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Network;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Clients
{
    /// <summary>
    /// 处理单个代理
    /// </summary>
    /// <param name="url"></param>
    public class ProxyHandler : IProxyHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyHandler));

        protected Proxy ProxyInfo { get; private set; }

        #region 协议相关
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
        #endregion

        #region 是否可用
        private bool _pingOk = false;
        /// <summary>
        /// 是否 ping 通
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEnable()
        {
            return ProxyInfo.IsActive && _pingOk;
        }

        public long Id { get; private set; }

        public async Task<bool> Ping(int pingCount = 3)
        {
            Ping2 ping = new(Host, pingCount);
            _pingOk = await ping.Ping();
            return _pingOk;
        }
        private Timer? _timer;
        /// <summary>
        /// 自动检测
        /// 每隔 1 分钟自动检测一次
        /// </summary>
        protected void AutoPing()
        {
            if (_timer != null) return;
            _timer = new Timer(async _ =>
            {
                await Ping();
            }, null, 0, 1000 * 60 * 1);
        }
        #endregion

        #region 构造函数
        public ProxyHandler(Proxy proxy)
        {

        }
        #endregion

        #region 使用历史记录
        private readonly ConcurrentDictionary<string, int> _usageCounter = new();
        /// <summary>
        /// 记录使用信息
        /// </summary>
        /// <param name="key"></param>
        protected virtual void RecordUsage(string key)
        {
            if (_usageCounter.TryGetValue(key, out var count))
            {
                _usageCounter[key] = count + 1;
            }
            else
            {
                _usageCounter.TryAdd(key, 1);
            }
        }
        #endregion
        /// <summary>
        /// 与字符串的转换
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Schema}://{Username}:{Password}@{Host}:{Port}";
        }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="matchStr"></param>
        /// <returns></returns>
        public virtual bool IsMatch(string matchStr, int limitCount)
        {
            // 为空时，默认全部匹配
            if (string.IsNullOrEmpty(ProxyInfo.MatchRegex)) return true;

            // 规则不匹配时，返回 false
            if (!Regex.IsMatch(matchStr, ProxyInfo.MatchRegex)) return false;

            // 次数超限判断
            if (_usageCounter.TryGetValue(matchStr, out var count))
            {
                return count < limitCount;
            }

            return true;
        }

        private ProxyClient _proxyClient;

        /// <summary>
        /// 生成代理客户端
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public virtual async Task<IProxyClient?> GetProxyClientAsync(string matchStr)
        {
            // 登记使用
            RecordUsage(matchStr);

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
                    _logger.Error($"不支持的代理协议: {Schema}");
                    break;
            }
            return _proxyClient;
        }

        public virtual void Update(Proxy proxy)
        {
            // 更新代理数据
            Id = proxy.Id;
            ProxyInfo = proxy;

            // 将字符串转换为代理
            Uri uri = new(proxy.Url);
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

            // 先 ping 一次
            Ping(1);
            AutoPing();
        }

        #region 静态方法
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
        #endregion
    }
}
