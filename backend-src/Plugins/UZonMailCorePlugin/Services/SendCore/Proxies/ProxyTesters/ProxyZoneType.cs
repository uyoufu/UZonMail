namespace UZonMail.Core.Services.SendCore.Proxies.ProxyTesters
{
    [Flags]
    public enum ProxyZoneType
    {
        /// <summary>
        /// 默认值，没有任何效果
        /// </summary>
        Default = 1,

        /// <summary>
        /// 表示能访问谷歌时调用
        /// </summary>
        Google = 2,

        /// <summary>
        /// 表示能访问 Baidu 时调用
        /// </summary>
        Baidu = 4,

        // 其它情况保留
    }
}
