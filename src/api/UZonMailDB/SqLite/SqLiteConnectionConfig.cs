using Microsoft.Data.Sqlite;
using UzonMail.DB.SQL;

namespace UzonMail.DB.SqLite
{
    /// <summary>
    /// SQLite 数据库连接配置。
    /// </summary>
    public class SqLiteConnectionConfig : IConnectionString
    {
        /// <summary>
        /// 是否启用 SQLite 数据库。
        /// </summary>
        public bool Enable { get; set; } = false;

        /// <summary>
        /// SQLite 数据库文件路径。
        /// </summary>
        public string DataSource { get; set; } = string.Empty;

        /// <summary>
        /// SQLite 缓存模式；未配置时使用驱动默认值。
        /// </summary>
        public SqliteCacheMode? Cache { get; set; }

        /// <summary>
        /// SQLite 打开模式；未配置时使用驱动默认值。
        /// </summary>
        public SqliteOpenMode? Mode { get; set; }

        /// <summary>
        /// SQLite 数据库密码。
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <inheritdoc />
        public string ConnectionString
        {
            get
            {
                var builder = new SqliteConnectionStringBuilder { DataSource = DataSource };

                if (Mode.HasValue)
                {
                    builder.Mode = Mode.Value;
                }

                if (Cache.HasValue)
                {
                    builder.Cache = Cache.Value;
                }

                if (!string.IsNullOrEmpty(Password))
                {
                    builder.Password = Password;
                }

                return builder.ConnectionString;
            }
        }
    }
}
