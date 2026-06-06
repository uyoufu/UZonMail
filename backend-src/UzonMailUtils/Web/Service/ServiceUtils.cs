using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UZonMail.Utils.Web.Service
{
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
            // 批量注入 Services 单例
            var assembleyTypes = servicesIn.GetTypes();
            var transientType = typeof(ITransientService);
            // 分多种情况，注册不同的生命周期

            // 瞬时类型
            var transientTypes = assembleyTypes
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => transientType.IsAssignableFrom(x))
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
            var scopedServiceTypes = assembleyTypes
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => scopedServiceType.IsAssignableFrom(x))
                .ToList();
            scopedServiceTypes.ForEach(type =>
            {
                var serviceTypes = GetServiceTypes(type);
                serviceTypes.ForEach(serviceType =>
                {
                    services.AddScoped(GetServiceType(serviceType), type);
                });
            });

            // 单例
            var singletonServiceType = typeof(ISingletonService);
            // 分多种情况，注册不同的生命周期
            var singletonServiceTypes = assembleyTypes
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => singletonServiceType.IsAssignableFrom(x))
                .ToList();
            singletonServiceTypes.ForEach(type =>
            {
                var serviceTypes = GetServiceTypes(type);
                serviceTypes.ForEach(serviceType =>
                {
                    services.AddSingleton(GetServiceType(serviceType), type);
                });
            });

            // 后台服务,在启动时，就会运行
            var hostedServiceType = typeof(IHostedService);
            var hostedServiceTypes = assembleyTypes
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => hostedServiceType.IsAssignableFrom(x))
                .ToList();
            hostedServiceTypes.ForEach(type =>
            {
                services.AddSingleton(hostedServiceType, type);
                services.AddSingleton(type);
                // 后台服务不支持泛型注册
            });

            return services;
        }

        /// <summary>
        /// 若是泛型，则获取泛型的开放类型 T<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetServiceType(Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition();
            return type;
        }

        /// <summary>
        /// 获取实现类型的服务类型列表
        /// </summary>
        /// <param name="implementationType">实现类型，必须继承 ITransientService 或 IScopedService 或 ISingletonService</param>
        /// <returns></returns>
        private static List<Type> GetServiceTypes(Type implementationType)
        {
            var interfaceNames = new List<Type>()
            {
                typeof(ITransientService<>),
                typeof(IScopedService<>),
                typeof(ISingletonService<>),
                typeof(ITransientService),
                typeof(IScopedService),
                typeof(ISingletonService)
            }.ConvertAll(x => x.Name);

            // 不断向上查找，直到找到 IService 为止
            // 不使用类型相等判断，是因为程序集可能在不同的上下文中，导致类型不相等
            var interfaces = implementationType
                .GetInterfaces()
                .Where(x => x.IsInterface)
                .Where(x => interfaceNames.Contains(x.Name))
                .ToList();

            List<Type> serviceTypes = [implementationType];
            foreach (var item in interfaces)
            {
                if (item.IsGenericType)
                {
                    // 获取泛型参数
                    var genericArguments = item.GetGenericArguments();
                    // 接口中的类型，可能也是泛型类型
                    foreach (var genericArg in genericArguments)
                    {
                        if (genericArg.IsGenericType)
                        {
                            serviceTypes.Add(genericArg.GetGenericTypeDefinition());
                        }
                        else
                        {
                            serviceTypes.AddRange(genericArguments);
                        }
                    }
                }
            }

            return serviceTypes;
        }
    }
}
