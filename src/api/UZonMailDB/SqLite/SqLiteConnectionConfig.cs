using Microsoft.Data.Sqlite;
using UzonMail.DB.SQL;

namespace UzonMail.DB.SqLite
{
    public class SqLiteConnectionConfig : IConnectionString
    {
        public bool Enable { get; set; } = false;
        public string DataSource { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Cache { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                SqliteConnectionStringBuilder builder =
                    new()
                    {
                        { "Data Source", DataSource },
                        { "Mode", Mode },
                        { "Cache", Cache },
                        { "Password", Password },
                        { "Version", Version }
                    };

                return builder.ConnectionString;
            }
        }
    }
}
