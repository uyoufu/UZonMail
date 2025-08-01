using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.MySql;
using UZonMail.DB.PostgreSql;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;

namespace UZonMail.DB.SQL
{
    public static class UseSqlExtension
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UseSqlExtension));
        /// <summary>
        /// 添加数据库上下文，优先使用 mysql
        /// 同时将 PostgreSql 或 MySqlContext 或 SqLiteContext 绑定到 SqlContext
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static IServiceCollection? AddSqlContext<TSqlContext, TPostgreSql, TMySqlContext, TSqLiteContext>(this IServiceCollection? services, IConfiguration configuration)
            where TSqlContext : SqlContextBase
            where TPostgreSql : TSqlContext
            where TMySqlContext : TSqlContext
            where TSqLiteContext : TSqlContext
        {
            if (services == null) return null;
            if (configuration == null) return services;

            if(AddPostgreSqlContext<TPostgreSql>(services, configuration))
            {
                // 将 PostgreSqlContext 绑定到 SqlContenxt
                services.AddScoped<TSqlContext, TPostgreSql>();
                return services;
            }

            // 优先使用 mysql
            if (AddMySqlContext<TMySqlContext>(services, configuration))
            {
                // 将 MySqlContext 绑定到 SqlContenxt
                services.AddScoped<TSqlContext, TMySqlContext>();
                return services;
            }

            if (AddSqLiteContext<TSqLiteContext>(services, configuration))
            {
                // 将 SqLiteContext 绑定到 SqlContenxt
                services.AddScoped<TSqlContext, TSqLiteContext>();
                return services;
            }

            throw new NullReferenceException("未找到任何数据库配置");
        }

        /// <summary>
        /// 添加 PostgreSql 数据库上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static bool AddPostgreSqlContext<T>(this IServiceCollection services, IConfiguration configuration) where T : DbContext
        {
            // 优先使用 mysql
            var connectionConfig = new PostgreSqlConnectionConfig();
            configuration.GetSection("Database:PostgreSql").Bind(connectionConfig);
            if (connectionConfig.Enable)
            {
                _logger.Info($"使用 PostgreSql 数据库: {connectionConfig.ConnectionString}");
                services.AddDbContext<T>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加 MySql 数据库上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static bool AddMySqlContext<T>(this IServiceCollection services, IConfiguration configuration) where T : DbContext
        {
            // 优先使用 mysql
            var mysqlConnectionConfig = new MySqlConnectionConfig();
            configuration.GetSection("Database:MySql").Bind(mysqlConnectionConfig);
            if (mysqlConnectionConfig.Enable)
            {
                _logger.Info($"使用 MySql 数据库: {mysqlConnectionConfig.ConnectionString}");
                services.AddDbContext<T>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加 SqLite 数据库上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static bool AddSqLiteContext<T>(this IServiceCollection services, IConfiguration configuration) where T : DbContext
        {
            var sqLiteConnectionConfig = new SqLiteConnectionConfig();
            configuration.GetSection("Database:SqLite").Bind(sqLiteConnectionConfig);
            if (sqLiteConnectionConfig.Enable)
            {
                _logger.Info($"使用 SqLite 数据库: {sqLiteConnectionConfig.ConnectionString}");
                services.AddDbContext<T>();
                return true;
            }
            return false;
        }
    }
}
