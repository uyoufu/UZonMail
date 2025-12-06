using System.Collections.Concurrent;
using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using Timer = System.Timers.Timer;

namespace UZonMail.CorePlugin.Services.SendCore.Sender
{
    /// <summary>
    /// 按邮箱域名及 IP 限制发送速率
    /// </summary>
    public class IPRateLimiter : ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IPRateLimiter));
        private readonly ConcurrentDictionary<string, DateTime> _lastSendedDateDic = new();
        private readonly Timer? _cleanupTimer;

        public IPRateLimiter()
        {
            // 定时释放过期的数据
            // 1h 清理一次
            _cleanupTimer = new Timer(1 * 60 * 60 * 1000);
            _cleanupTimer.Elapsed += CleanupTimer_Elapsed;
        }

        private void CleanupTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var keys = _lastSendedDateDic.Keys.ToList();
            foreach (var key in keys)
            {
                if (_lastSendedDateDic.TryGetValue(key, out var lastDate))
                {
                    // 超过 1 小时未发送，认为过期
                    if ((DateTime.UtcNow - lastDate).TotalHours > 1)
                    {
                        _lastSendedDateDic.TryRemove(key, out _);
                    }
                }
            }
        }

        /// <summary>
        /// 等待发送
        /// </summary>
        /// <param name="context"></param>
        /// <param name="outbox"></param>
        /// <param name="hostIp"></param>
        /// <returns></returns>
        public async Task WaitForReleaseAsync(SendingContext context, string outbox, string? hostIp)
        {
            var settingsManager = context.Provider.GetRequiredService<AppSettingsManager>();
            var sendingSetting = await settingsManager.GetSetting<SendingSetting>(
                context.SqlContext,
                context.OutboxAddress.UserId
            );
            await WaitForReleaseAsync(outbox, hostIp, sendingSetting.MaxCountPerIPDomainHour);
        }

        /// <summary>
        /// 等待发送
        /// </summary>
        /// <param name="outbox"></param>
        /// <param name="hostIp"></param>
        /// <returns></returns>
        public async Task WaitForReleaseAsync(
            string outbox,
            string? hostIp,
            int maxCountPerIPDomainHour = 0
        )
        {
            if (maxCountPerIPDomainHour <= 0)
                return;

            var cooldownMilliseconds = 60 * 60 * 1000 / maxCountPerIPDomainHour;

            var key = GetKey(outbox, hostIp);
            if (_lastSendedDateDic.TryGetValue(key, out var lastDate))
            {
                var timeSinceLastSend = DateTime.UtcNow - lastDate;
                if (timeSinceLastSend.TotalMilliseconds < cooldownMilliseconds)
                {
                    var waitTime = cooldownMilliseconds - (int)timeSinceLastSend.TotalMilliseconds;
                    _logger.Info($"等待 {waitTime} 毫秒以满足发送速率限制，Outbox: {outbox}, HostIp: {hostIp}");
                    await Task.Delay(waitTime);
                }

                // 更新发送时间
                _lastSendedDateDic[key] = DateTime.UtcNow;
            }
            else
            {
                _lastSendedDateDic.TryAdd(key, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// 是否已经达到限制
        /// </summary>
        /// <param name="outbox"></param>
        /// <param name="hostIp"></param>
        /// <param name="maxCountPerIPDomainHour"></param>
        /// <returns></returns>
        public bool IsLimited(string outbox, string? hostIp, int maxCountPerIPDomainHour = 0)
        {
            if (maxCountPerIPDomainHour <= 0)
                return false;

            var cooldownMilliseconds = 60 * 60 * 1000 / maxCountPerIPDomainHour;

            var key = GetKey(outbox, hostIp);
            if (_lastSendedDateDic.TryGetValue(key, out var lastDate))
            {
                var timeSinceLastSend = DateTime.UtcNow - lastDate;
                return timeSinceLastSend.TotalMilliseconds < cooldownMilliseconds;
            }

            return false;
        }

        private static string GetKey(string outbox, string? hostIp)
        {
            var domain = outbox.Split('@').Last();
            if (string.IsNullOrEmpty(hostIp))
                return domain;
            return $"{domain}_{hostIp}";
        }
    }
}
