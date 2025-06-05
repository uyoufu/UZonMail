namespace UZonMail.Core.Services.SendCore.DynamicProxy.ProxyTesters
{
    [Flags]
    public enum ProxyTesterType
    {
        /// <summary>
        /// 所有类型的代理查询都可以使用
        /// </summary>
        All = 1,

        /// <summary>
        /// 表示能访问谷歌时调用
        /// </summary>
        Google = 2,

        /// <summary>
        /// 表示能访问 Baidu 时调用
        /// </summary>
        Baidu = 3,

        // 其它情况保留
    }
}
