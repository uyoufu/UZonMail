using Microsoft.EntityFrameworkCore;
using UZonMail.DB.MySql;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;

namespace UZonMail.DB.SQL
{
    public static class UseSqlExtension
    {
        /// <summary>
        /// 添加数据库上下文，优先使用 mysql
        /// 同时将 MySqlContext 或 SqLiteContext 绑定到 SqlContext
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static IServiceCollection? AddSqlContext<TSqlContext, TMySqlContext, TSqLiteContext>(this IServiceCollection? services, IConfiguration configuration)
            where TSqlContext : SqlContextBase
            where TMySqlContext : TSqlContext
            where TSqLiteContext : TSqlContext
        {
            if (services == null) return null;
            if (configuration == null) return services;

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
                services.AddDbContext<T>();
                return true;
            }
            return false;
        }
    }
}
