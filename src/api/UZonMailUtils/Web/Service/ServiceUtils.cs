using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace UzonMail.Utils.Web.Service
{
    /// <summary>
    /// 根据服务标记接口批量注册程序集中的应用服务。
    /// </summary>
    public class ServiceUtils
    {
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
        /// <param name="services"></param>
        /// <param name="servicesIn">要扫描的程序集</param>
        /// <returns></returns>
        public static IServiceCollection AddServices(
            IServiceCollection services,
            Assembly servicesIn
        )
        {
            var assemblyTypes = servicesIn
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();

            AddMarkerServices(
                services,
                assemblyTypes,
                typeof(ITransientService),
                typeof(ITransientService<>),
                ServiceLifetime.Transient
            );
            AddMarkerServices(
                services,
                assemblyTypes,
                typeof(IScopedService),
                typeof(IScopedService<>),
                ServiceLifetime.Scoped
            );
            AddMarkerServices(
                services,
                assemblyTypes,
                typeof(ISingletonService),
                typeof(ISingletonService<>),
                ServiceLifetime.Singleton
            );

            foreach (
                var hostedServiceImplementation in assemblyTypes.Where(type =>
                    typeof(IHostedService).IsAssignableFrom(type)
                )
            )
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Singleton(typeof(IHostedService), hostedServiceImplementation)
                );
                services.TryAdd(
                    ServiceDescriptor.Singleton(
                        hostedServiceImplementation,
                        hostedServiceImplementation
                    )
                );
            }

            return services;
        }

        private static void AddMarkerServices(
            IServiceCollection services,
            IReadOnlyList<Type> assemblyTypes,
            Type markerType,
            Type genericMarkerType,
            ServiceLifetime lifetime
        )
        {
            foreach (var implementationType in assemblyTypes.Where(markerType.IsAssignableFrom))
            {
                foreach (var serviceType in GetServiceTypes(implementationType, genericMarkerType))
                {
                    var descriptor = new ServiceDescriptor(
                        serviceType,
                        implementationType,
                        lifetime
                    );
                    if (serviceType == implementationType)
                    {
                        // 显式注册应拥有最终决定权，自动扫描不能覆盖数据库上下文等定制映射。
                        services.TryAdd(descriptor);
                        continue;
                    }

                    services.TryAddEnumerable(descriptor);
                }
            }
        }

        private static IReadOnlyList<Type> GetServiceTypes(
            Type implementationType,
            Type genericMarkerType
        )
        {
            return
            [
                implementationType,
                .. implementationType
                    .GetInterfaces()
                    .Where(type =>
                        type.IsGenericType && type.GetGenericTypeDefinition() == genericMarkerType
                    )
                    .SelectMany(type => type.GetGenericArguments())
                    .Select(type => type.IsGenericType ? type.GetGenericTypeDefinition() : type)
                    .Distinct(),
            ];
        }
    }
}
