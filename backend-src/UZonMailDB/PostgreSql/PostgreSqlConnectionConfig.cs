using UZonMail.DB.SQL;

namespace UZonMail.DB.PostgreSql
{
    /// <summary>
    /// mysql 连接信息
    /// </summary>
    public class PostgreSqlConnectionConfig : IConnectionString
    {
        public bool Enable { get; set; } = false;
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString => $"Host={Host};Port={Port};Database={Database};Username={User};Password={Password};";        
    }
}
