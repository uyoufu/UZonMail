using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;

namespace UZonMail.Utils.Log
{
    public static class LoggerExtension
    {
        /// <summary>
        /// 将 logging 中的日志级别映射到 log4net
        /// </summary>
        /// <param name="builder"></param>
        public static void AttachLevelToLog4Net(this ILoggingBuilder builder,string level)
        {            
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var rootLogger = hierarchy.Root;
            rootLogger.Level = hierarchy.LevelMap[level.ToUpper()];
            hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
        }
    }
}
