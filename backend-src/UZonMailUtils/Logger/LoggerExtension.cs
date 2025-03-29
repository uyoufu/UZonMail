using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace UZonMail.Utils.Log
{
    public static class LoggerExtension
    {
        /// <summary>
        /// 将 logging 中的日志级别映射到 log4net
        /// </summary>
        /// <param name="builder"></param>
        public static void AttachLevelToLog4Net(this IHostApplicationBuilder builder)
        {
            var logLevel = builder.Configuration.GetSection("Logging:LogLevel:Default").Get<LogLevel>();

            var log4netLevelNames = new List<string>() { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "CRITICAL", "ALL" };

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var rootLogger = hierarchy.Root;
            var level = hierarchy.LevelMap[log4netLevelNames[(int)logLevel]];
            if (level == null)
            {
                // 未识别到，不修改日志等级
                return;
            }
            rootLogger.Level = level;
            hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
        }
    }
}
