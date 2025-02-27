using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;

namespace UZonMail.DB.SqLite
{
    public class SqLiteContext : SqlContext
    {
        private readonly SqLiteConnectionConfig _sqLiteConnectionConfig;

        internal SqLiteContext(DbContextOptions<SqlContext> options) : base(options)
        {
        }

        public SqLiteContext(IConfiguration configuration)
        {
            // sqlLite
            _sqLiteConnectionConfig = new SqLiteConnectionConfig();
            configuration.GetSection("Database:SqLite").Bind(_sqLiteConnectionConfig);

            var sqlLiteFilePath = _sqLiteConnectionConfig.DataSource;
            if (!string.IsNullOrEmpty(sqlLiteFilePath))
            {
                var directory = Path.GetDirectoryName(sqlLiteFilePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (_sqLiteConnectionConfig != null)
                options.UseSqlite(_sqLiteConnectionConfig.ConnectionString);
        }
    }
}
