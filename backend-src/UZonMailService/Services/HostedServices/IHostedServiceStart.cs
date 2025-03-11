namespace UZonMailService.Services.PostStartup
{
    public interface IHostedServiceStart
    {
        /// <summary>
        /// 优先级
        /// </summary>
        int Order { get; }

        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
