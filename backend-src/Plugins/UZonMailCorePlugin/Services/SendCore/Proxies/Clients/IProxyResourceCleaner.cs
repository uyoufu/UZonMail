namespace UZonMail.CorePlugin.Services.SendCore.Proxies.Clients
{
    /// <summary>
    /// 释放或清理代理处理器内部的过期资源。
    /// </summary>
    public interface IProxyResourceCleaner
    {
        void CleanupExpiredResources();
    }
}
