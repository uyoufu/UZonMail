using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Core.Config.SubConfigs
{
    public class DatabaseConfig
    {
        /// <summary>
        /// liteDB 保存的相对路径
        /// </summary>
        public string LiteDbPath { get; set; }

        /// <summary>
        /// SQLite 连接字符串
        /// </summary>
        public string SqliteConnectionString { get; set; }

        #region 静态帮助方法
        private static string GetDatabaseConfigKey()
        {
            return $"{nameof(DatabaseConfig).Replace("Config", "")}";
        }

        public static string GetSqliteConnectionStringConfigKey()
        {
            return $"{GetDatabaseConfigKey()}:{nameof(SqliteConnectionString)}";
        }

        /// <summary>
        /// 获取 LiteDB 相对路径的配置键
        /// </summary>
        /// <returns></returns>
        public static string GetLiteDbPathConfigKey()
        {
            return $"{GetDatabaseConfigKey()}:{nameof(LiteDbPath)}";
        }
        #endregion
    }
}
