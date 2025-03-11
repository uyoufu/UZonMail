using UZonMail.DB.SQL;

namespace UZonMail.DB.MySql
{
    /// <summary>
    /// mysql 连接信息
    /// </summary>
    public class MySqlConnectionConfig : IConnectionString
    {
        public bool Enable { get; set; } = false;
        public string Version { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString => $"Server={Host};Port={Port};Database={Database};User={User};Password={Password};";
        /// <summary>
        /// Mysql 版本号
        /// </summary>
        public Version MysqlVersion => new(Version);
    }
}
