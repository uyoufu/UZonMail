using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using UZonMail.DB.SQL;

namespace UZonMail.DB.MySql
{
    public class MySqlContext : SqlContext
    {
        private readonly IConfiguration _configuration;

        internal MySqlContext(DbContextOptions<SqlContext> options) : base(options)
        {
        }

        [ActivatorUtilitiesConstructor]
        public MySqlContext(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            SqlContextHelper.ConfiguringMySql(options, _configuration);
        }
    }
}
