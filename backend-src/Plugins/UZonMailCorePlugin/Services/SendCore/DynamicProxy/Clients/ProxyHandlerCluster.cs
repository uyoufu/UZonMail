﻿using log4net;
using MailKit.Net.Proxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Clients
{
    /// <summary>
    /// 代理客户端集群基类
    /// </summary>
    public abstract class ProxyHandlerCluster : ProxyHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyHandlerCluster));
        private List<ProxyHandler> _handlers = [];

        /// <summary>
        /// 最小数量
        /// 当小于这个数量时，就会重新获取代理
        /// </summary>
        private readonly int _minimumCount = 3;

        /// <summary>
        /// 获取 IP 地址
        /// </summary>
        /// <returns></returns>
        protected abstract Task<List<ProxyHandler>> GetProxyHandlersAsync(IServiceProvider serviceProvider);

        public override async Task<ProxyClientAdapter?> GetProxyClientAsync(IServiceProvider serviceProvider, string matchStr)
        {
            // 移除不可用的代理客户端
            _handlers.RemoveAll(handler => !handler.IsEnable());

            // 判断是否有可用代理客户端，若没有，则更新
            if (_handlers.Count < _minimumCount)
            {
                var updateTask = UpdateProxyHandlers(serviceProvider);
                if (_handlers.Count == 0)
                {
                    await updateTask;
                }
            }

            if (_handlers.Count == 0)
            {
                _logger.Warn("没有可用的代理客户端");
                return null;
            }

            // 随机选择一个代理客户端
            var handler = _handlers[new Random().Next(0, _handlers.Count)];
            return await handler.GetProxyClientAsync(serviceProvider,matchStr);
        }

        private async Task UpdateProxyHandlers(IServiceProvider serviceProvider)
        {
            var handlers = await GetProxyHandlersAsync(serviceProvider);
            // 可能存在重复的代理
            foreach (var handler in handlers)
            {
                var existOne = _handlers.FirstOrDefault(x => x.Id == handler.Id);
                if (existOne != null)
                {
                    // 更新代理信息
                    existOne.Update(handler.ProxyInfo);
                }
                else
                {
                    _handlers.Add(handler);
                }
            }
        }

        public override bool IsMatch(string matchStr, int limitCount)
        {
            if (string.IsNullOrEmpty(ProxyInfo.MatchRegex)) return true;

            // 规则匹配
            return Regex.IsMatch(matchStr, ProxyInfo.MatchRegex);
        }

        /// <summary>
        /// 始终可用
        /// </summary>
        /// <returns></returns>
        public override bool IsEnable()
        {
            return true;
        }

        /// <summary>
        /// 那张健康状态
        /// </summary>
        public override void MarkHealthless()
        {            
        }

        /// <summary>
        /// IP 池不检测健康状态
        /// </summary>
        /// <returns></returns>
        protected override async Task<bool> HealthCheck()
        {
            return true;
        }

        protected override void AutoHealthCheck()
        {
            // 不做任何操作
        }
    }
}
