using UZonMail.DB.MySql;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;

namespace UZonMail.DB.SQL
{
    public static class UseSqlExtension
    {
        public static IServiceCollection? AddSqlContext(this IServiceCollection? services)
        {
            if(services == null) return null;

            // 根据配置文件选择数据库
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>() ?? throw new NotFiniteNumberException("未找到 IConfiguration 服务");

            // 优先使用 mysql
            var mysqlConnectionConfig = new MySqlConnectionConfig();
            configuration.GetSection("Database:MySql").Bind(mysqlConnectionConfig);
            if (mysqlConnectionConfig.Enable)
            {
                // 将 MySqlContext 绑定到 SqlContenxt
                services.AddScoped<SqlContext, MySqlContext>();
                services.AddDbContext<MySqlContext>();
                return services;
            }

            // 使用 sqlite
            var sqLiteConnectionConfig = new SqLiteConnectionConfig();
            configuration.GetSection("Database:SqLite").Bind(sqLiteConnectionConfig);
            if(sqLiteConnectionConfig.Enable)
            {
                // 将 SqLiteContext 绑定到 SqlContenxt
                services.AddScoped<SqlContext, SqLiteContext>();
                services.AddDbContext<SqLiteContext>();
                return services;
            }

            throw new ArgumentException("未找到数据库配置");  
        }
    }
}
