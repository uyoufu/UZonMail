using UzonMail.DB.SQL;

namespace UzonMail.DB.PostgreSql
{
    /// <summary>
    /// PostgreSql 连接信息
    /// </summary>
    public class PostgreSqlConnectionConfig : IConnectionString
    {
        public bool Enable { get; set; } = false;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString =>
            $"Host={Host};Port={Port};Database={Database};Username={User};Password={Password};";
    }
}
