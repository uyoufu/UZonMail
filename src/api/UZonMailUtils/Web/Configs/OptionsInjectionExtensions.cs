using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace UzonMail.Utils.Web.Configs
{
    public static class OptionsInjectionExtensions
    {
        public static IServiceCollection AddAllOptions(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions();

            var optionTypes = GetCandidateAssemblies()
                .SelectMany(GetTypes)
                .Where(type =>
                    type.IsClass
                    && !type.IsAbstract
                    && !type.ContainsGenericParameters
                    && typeof(IAppOptions).IsAssignableFrom(type)
                )
                .OrderBy(type => type.FullName, StringComparer.Ordinal);

            foreach (var optionType in optionTypes)
            {
                RegisterOptions(services, configuration, optionType);
            }

            return services;
        }

        private static IEnumerable<Assembly> GetCandidateAssemblies()
        {
            var markerAssembly = typeof(IAppOptions).Assembly;
            var markerAssemblyName = markerAssembly.GetName();

            return AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Where(assembly =>
                    assembly == markerAssembly
                    || assembly
                        .GetReferencedAssemblies()
                        .Any(reference =>
                            AssemblyName.ReferenceMatchesDefinition(reference, markerAssemblyName)
                        )
                );
        }

        private static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                throw new InvalidOperationException(
                    $"无法扫描程序集 {assembly.FullName} 中的 Options 类型。",
                    exception
                );
            }
        }

        private static void RegisterOptions(
            IServiceCollection services,
            IConfiguration configuration,
            Type optionType
        )
        {
            if (optionType.GetConstructor(Type.EmptyTypes) is null)
            {
                throw new InvalidOperationException(
                    $"Options 类型 {optionType.FullName} 必须具有公共无参构造函数。"
                );
            }

            var section = configuration.GetSection(AppOptionsHelper.GetOptionKey(optionType));
            var configureServiceType = typeof(IConfigureOptions<>).MakeGenericType(optionType);
            var configureImplementationType =
                typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionType);
            var changeTokenServiceType = typeof(IOptionsChangeTokenSource<>).MakeGenericType(
                optionType
            );
            var changeTokenImplementationType =
                typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionType);

            var configureOptions = Activator.CreateInstance(
                configureImplementationType,
                Options.DefaultName,
                section
            )!;
            var changeTokenSource = Activator.CreateInstance(
                changeTokenImplementationType,
                Options.DefaultName,
                section
            )!;

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton(configureServiceType, configureOptions)
            );
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton(changeTokenServiceType, changeTokenSource)
            );
        }
    }
}
