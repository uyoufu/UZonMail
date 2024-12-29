namespace UZonMail.Core.Services.Cache
{
    /// <summary>
    /// Redis 连接配置
    /// </summary>
    public class RedisConnectionConfig
    {
        public bool Enable { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string? Password { get; set; }
        public int Database { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString => $"{Host}:{Port},password={Password},defaultDatabase={Database}";
    }
}
