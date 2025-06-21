using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using UZonMail.Utils.Web.Service;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using UZonMail.Utils.Web.Convention;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using UZonMail.Utils.Database.Redis;

namespace UZonMail.Utils.Web
{
    /// <summary>
    /// DotNETCore 扩展类
    /// </summary>
    public static class Setup
    {
        /// <summary>
        /// 设置 slugify-case 形式的路由
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection SetupSlugifyCaseRoute(this IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(
                                             new SlugifyParameterTransformer()));
            });
            return services;
        }

        /// <summary>
        /// 批量注入调用程序集中的服务
        /// 该方法会自动扫描当前程序集中的所有服务，并注册到容器中, 服务包括实现了以下接口的类：
        /// 1. ITransientService
        /// 2. IScopedService
        /// 3. ISingletonService
        /// 4. IHostedService
        /// 其中，ITransientService、IScopedService、ISingletonService 可以是泛型接口
        /// IHostedService 是后台服务，会自动注册为单例
        /// </summary>
        /// <typeparam name="T">通过指定类型,来注入所有实现该接口的单例。若要全部注册，只需传入 IService 即可</typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // 批量注入 Services 单例            
            var assembleyTypes = Assembly.GetCallingAssembly()
                .GetTypes();
            var transientType = typeof(ITransientService);
            // 分多种情况，注册不同的生命周期

            // 瞬时类型
            var transientTypes = assembleyTypes.Where(x => !x.IsInterface && !x.IsAbstract && transientType.IsAssignableFrom(x))
                .ToList();
            transientTypes.ForEach(type =>
            {
                // 获取注册类型和实现类型
                var serviceTypes = GetServiceTypes(type);
                serviceTypes.ForEach(serviceType =>
                {
                    services.AddTransient(serviceType, type);
                });
            });

            // 请求周期
            var scopedServiceType = typeof(IScopedService);
            // 分多种情况，注册不同的生命周期
            var scopedServiceTypes = assembleyTypes.Where(x => !x.IsInterface && !x.IsAbstract && scopedServiceType.IsAssignableFrom(x))
                .ToList();
            scopedServiceTypes.ForEach(type =>
            {
                var serviceTypes = GetServiceTypes(type);
                serviceTypes.ForEach(serviceType =>
                {
                    services.AddScoped(serviceType, type);
                });
            });

            // 单例
            var singletonServiceType = typeof(ISingletonService);
            // 分多种情况，注册不同的生命周期
            var singletonServiceTypes = assembleyTypes.Where(x => !x.IsInterface && !x.IsAbstract && singletonServiceType.IsAssignableFrom(x))
               .ToList();
            singletonServiceTypes.ForEach(type =>
            {
                var serviceTypes = GetServiceTypes(type);
                serviceTypes.ForEach(serviceType =>
                {
                    services.AddSingleton(serviceType, type);
                });
            });

            // 后台服务,在启动时，就会运行
            var hostedServiceType = typeof(IHostedService);
            var hostedServiceTypes = assembleyTypes.Where(x => !x.IsInterface && !x.IsAbstract && hostedServiceType.IsAssignableFrom(x))
                .ToList();
            hostedServiceTypes.ForEach(type =>
            {
                services.AddSingleton(hostedServiceType, type);
                services.AddSingleton(type);
            });

            return services;
        }

        /// <summary>
        /// 通过实现类型获取服务类型
        /// </summary>
        /// <param name="implementationType">实现类型，必须继承 ITransientService 或 IScopedService 或 ISingletonService</param>
        /// <returns></returns>
        private static List<Type> GetServiceTypes(Type implementationType)
        {
            var interfaceNames = new List<Type>() {
                typeof(ITransientService<>), typeof(IScopedService<>), typeof(ISingletonService<>),
                typeof(ITransientService), typeof(IScopedService), typeof(ISingletonService)
            }.ConvertAll(x => x.Name);

            // 不断向上查找，直到找到 IService 为止
            var interfaces = implementationType.GetInterfaces()
                .Where(x => x.IsInterface)
                .Where(x => interfaceNames.Contains(x.Name))
                .ToList();

            List<Type> serviceTypes = [implementationType];
            foreach (var item in interfaces)
            {
                if (item.IsGenericType)
                {
                    // 获取泛型参数
                    Type genericArgument = item.GetGenericArguments().First();
                    serviceTypes.Add(genericArgument);
                }
            }

            return serviceTypes;
        }

        /// <summary>
        /// 配置 swagger
        /// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        /// </summary>
        /// <param name="services"></param>
        /// <param name="apiInfo"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerGen(this IServiceCollection services, OpenApiInfo apiInfo, string xmlCommentsPath)
        {
            services.AddSwaggerGen(swaggerOptions =>
            {
                swaggerOptions.SwaggerDoc("v1", apiInfo);

                // Set the comments path for the Swagger JSON and UI.    
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsPath);
                swaggerOptions.IncludeXmlComments(xmlPath);

                // Bearer 的scheme定义
                var securityScheme = new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    //参数添加在头部
                    In = ParameterLocation.Header,
                    //使用Authorize头部
                    Type = SecuritySchemeType.Http,
                    //内容为以 bearer开头
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                //把所有方法配置为增加bearer头部信息
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                           Reference = new OpenApiReference
                           {
                               Type = ReferenceType.SecurityScheme,
                               Id = "bearerAuth"
                           }
                        },
                        Array.Empty<string>()
                    }
                };

                //注册到swagger中
                swaggerOptions.AddSecurityDefinition("bearerAuth", securityScheme);
                swaggerOptions.AddSecurityRequirement(securityRequirement);
            });

            return services;
        }

        /// <summary>
        /// 配置 jwt 验证
        /// 参考：https://learn.microsoft.com/zh-cn/aspnet/core/signalr/authn-and-authz?view=aspnetcore-8.0
        /// </summary>
        /// <param name="services"></param>
        /// <param name="secretKey"></param>
        /// <param name="redisConnection">若传递该参数，会进行 token 黑名单验证</param>
        /// <returns></returns>
        public static IServiceCollection AddJWTAuthentication(this IServiceCollection services, string secretKey, RedisConnectionConfig? redisConnection = null)
        {
            // reids 缓存
            RedisCacheAdapter? redisCache = null;
            if (redisConnection != null && redisConnection.Enable)
            {
                // 初始化 Redis
                redisCache = new RedisCacheAdapter(redisConnection);
            }

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // 是否验证令牌有效期
                    ValidateLifetime = true,
                    // 每次颁发令牌，令牌有效时间
                    ClockSkew = TimeSpan.FromMinutes(1440)
                };

                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.

                // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
                // due to a limitation in Browser APIs. We restrict it to only calls to the
                // SignalR hub in this code.
                // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                // for more information about security considerations when using
                // the query string to transmit the access token.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Token == null)
                        {
                            var accessToken = context.Request.Query["access_token"];
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        if (redisCache == null || !redisCache.Enable) return;
                        var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                        if (string.IsNullOrEmpty(jti)) return;

                        // 查询黑名单
                        var isBlacklisted = await redisCache.KeyExistsAsync($"jwt:blacklist:{jti}");
                        if (isBlacklisted)
                        {
                            context.Fail("Token has been deprecated!");
                        }
                    }
                };
            });
            return services;
        }

        /// <summary>
        /// 判断是否有同类型的进程，若有，关闭
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseSingleApp(this IServiceCollection services)
        {
            // 这种方式可能导致数据库迁移失败，因此迁移时不使用
            bool isMigration = Environment.GetEnvironmentVariable("EF_MIGRATIONS") == "true";
            if (isMigration)
                return services;

            // 获取当前程序名称
            var currentProcess = Process.GetCurrentProcess();
            // 查找同名进程并关闭
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var process in processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    process.Kill();
                }
            }
            return services;
        }
    }
}
