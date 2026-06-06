using log4net;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.PostgreSql;
using UzonMail.DB.SqLite;

namespace UzonMail.DB.SQL
{
    public static class UseSqlExtension
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UseSqlExtension));

        /// <summary>
        /// 添加数据库上下文，优先使用 PostgreSql
        /// 同时将 PostgreSqlContext 或 SqLiteContext 绑定到 SqlContext
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static IServiceCollection? AddSqlContext<TSqlContext, TPostgreSql, TSqLiteContext>(
            this IServiceCollection? services,
            IConfiguration configuration
        )
            where TSqlContext : SqlContextBase
            where TPostgreSql : TSqlContext
            where TSqLiteContext : TSqlContext
        {
            if (services == null)
                return null;
            if (configuration == null)
                return services;

            if (AddPostgreSqlContext<TPostgreSql>(services, configuration))
            {
                // 将 PostgreSqlContext 绑定到 SqlContenxt
                services.AddScoped<TSqlContext, TPostgreSql>();
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
        private static bool AddPostgreSqlContext<T>(
            this IServiceCollection services,
            IConfiguration configuration
        )
            where T : DbContext
        {
            // 优先使用 PostgreSql
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
        /// 添加 SqLite 数据库上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static bool AddSqLiteContext<T>(
            this IServiceCollection services,
            IConfiguration configuration
        )
            where T : DbContext
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
