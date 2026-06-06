using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;

namespace UzonMail.DB.PostgreSql
{
    public class PostgreSqlContext : SqlContext
    {
        private readonly IConfiguration _configuration;

        internal PostgreSqlContext(DbContextOptions<SqlContext> options) : base(options)
        {
        }

        [ActivatorUtilitiesConstructor]
        public PostgreSqlContext(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            SqlContextHelper.ConfiguringPostgreSql(options, _configuration);
        }
    }
}
