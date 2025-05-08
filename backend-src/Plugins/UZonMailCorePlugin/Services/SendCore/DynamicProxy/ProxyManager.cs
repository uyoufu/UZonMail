using log4net;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.DynamicProxy
{
    /// <summary>
    /// 代理管理器
    /// </summary>
    public class ProxyManager : ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyManager));
        private readonly AppSettingsManager _settingsService;

        public ProxyManager(AppSettingsManager settingsService)
        {
            _settingsService = settingsService;

            DisposeHandlerAuto();
        }

        private readonly ConcurrentDictionary<long, UserProxyManager> _userProxyManagers = new();

        /// <summary>
        /// 重新加载用户的代理
        /// </summary>
        /// <returns></returns>
        public async Task UpdateUserProxies(IServiceProvider serviceProvider, long userId)
        {
            if (_userProxyManagers.TryGetValue(userId, out var value))
            {
                await value.UpdateProxies(serviceProvider);
            }
        }


        /// <summary>
        /// 获取代理处理器
        /// </summary>
        /// <param name="userId">用户ID</param></param>
        /// <param name="matchStr">匹配字符，比如发件箱号</param>
        /// <returns></returns>
        public async Task<IProxyHandler?> GetProxyHandler(SendingContext sendingContext)
        {
            if (sendingContext.EmailItem == null) return null;

            var userId = sendingContext.EmailItem.UserId;
            var outboxEmail = sendingContext.EmailItem.Outbox.Email;

            if (!_userProxyManagers.TryGetValue(userId, out var manager))
            {
                // 新增并添加
                manager = new UserProxyManager(userId);
                await manager.UpdateProxies(sendingContext.Provider);
                _userProxyManagers.TryAdd(userId, manager);
            }

            if (sendingContext.EmailItem.ProxyId > 0)
            {
                // 返回特定的代理
                return manager.GetProxyHandler(sendingContext.EmailItem.ProxyId);
            }

            var orgSetting = await _settingsService.GetSetting<SendingSetting>(sendingContext.SqlContext, userId);            
            // 随机匹配代理
            return manager.RandomProxyHandler(outboxEmail, orgSetting.ChangeIpAfterEmailCount, sendingContext.EmailItem.AvailableProxyIds);
        }

        /// <summary>
        /// 随机匹配用户下的代理
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="userId"></param>
        /// <param name="matchStr"></param>
        /// <returns></returns>
        public async Task<IProxyHandler?> GetProxyHander(IServiceProvider serviceProvider, long userId,string matchStr)
        {
            if (!_userProxyManagers.TryGetValue(userId, out var manager))
            {
                // 新增并添加
                manager = new UserProxyManager(userId);
                await manager.UpdateProxies(serviceProvider);
                _userProxyManagers.TryAdd(userId, manager);
            }

            var sqlContext = serviceProvider.GetRequiredService<SqlContext>();
            var orgSetting = await _settingsService.GetSetting<SendingSetting>(sqlContext, userId);

            // 随机匹配代理
            return manager.RandomProxyHandler(matchStr, orgSetting.ChangeIpAfterEmailCount);
        }


        private Timer? _timer;
        /// <summary>
        /// 释放资源
        /// </summary>
        private void DisposeHandlerAuto()
        {
            if (_timer != null) return;

            _timer = new Timer(_ =>
            {
                _logger.Info("开始自动释放代理处理器");
                foreach (var manager in _userProxyManagers.Values)
                {
                    manager.DisposeHandler();
                }
            }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }
    }
}
