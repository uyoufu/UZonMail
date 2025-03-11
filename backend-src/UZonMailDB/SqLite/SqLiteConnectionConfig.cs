using Microsoft.Data.Sqlite;
using UZonMail.DB.SQL;

namespace UZonMail.DB.SqLite
{
    public class SqLiteConnectionConfig : IConnectionString
    {
        public bool Enable { get; set; } = false;
        public string DataSource { get; set; }
        public string Version { get; set; }
        public string Cache { get; set; }
        public string Mode { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                SqliteConnectionStringBuilder builder = new()
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
