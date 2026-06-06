namespace UZonMail.CorePlugin.Services.SendCore.Proxies.Clients
{
    /// <summary>
    /// 可由代理管理器集中维护健康状态的代理处理器。
    /// </summary>
    public interface IProxyHealthCheckable
    {
        bool ShouldHealthCheck { get; }

        Task<bool> HealthCheck();
    }
}
