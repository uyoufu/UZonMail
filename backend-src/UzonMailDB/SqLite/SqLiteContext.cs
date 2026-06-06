using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;

namespace UZonMail.DB.SqLite
{
    public class SqLiteContext : SqlContext
    {
        private readonly IConfiguration _configuration;

        internal SqLiteContext(DbContextOptions<SqlContext> options) : base(options)
        {
        }

        [ActivatorUtilitiesConstructor]
        public SqLiteContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            SqlContextHelper.ConfiguringSqLite(options, _configuration);            
        }
    }
}
