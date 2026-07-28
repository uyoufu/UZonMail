using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace UzonMail.Utils.Plugin
{
    internal sealed record ManagedAssemblyMetadata(
        string Path,
        AssemblyName Identity,
        Guid ModuleVersionId,
        IReadOnlySet<string> ReferencedAssemblyNames
    )
    {
        public string Name =>
            Identity.Name ?? throw new InvalidOperationException($"程序集缺少名称: {Path}");
    }

    internal sealed record DuplicatePluginAssembly(
        string AssemblyName,
        string CanonicalPath,
        IReadOnlyList<string> DuplicatePaths
    );

    /// <summary>
    /// 扫描插件目录并生成不执行程序集代码的元数据目录。
    /// </summary>
    internal sealed class PluginAssemblyCatalog
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<ManagedAssemblyMetadata>> _files;
        private readonly IReadOnlyDictionary<string, ManagedAssemblyMetadata> _plugins;

        private PluginAssemblyCatalog(
            IReadOnlyDictionary<string, IReadOnlyList<ManagedAssemblyMetadata>> files,
            IReadOnlyDictionary<string, ManagedAssemblyMetadata> plugins,
            IReadOnlyList<DuplicatePluginAssembly> duplicatePlugins
        )
        {
            _files = files;
            _plugins = plugins;
            DuplicatePlugins = duplicatePlugins;
        }

        public IReadOnlyList<ManagedAssemblyMetadata> Plugins => [.. _plugins.Values];

        public IReadOnlyList<DuplicatePluginAssembly> DuplicatePlugins { get; }

        public static PluginAssemblyCatalog CreateEmpty()
        {
            return new PluginAssemblyCatalog(
                new Dictionary<string, IReadOnlyList<ManagedAssemblyMetadata>>(
                    StringComparer.OrdinalIgnoreCase
                ),
                new Dictionary<string, ManagedAssemblyMetadata>(StringComparer.OrdinalIgnoreCase),
                []
            );
        }

        public static PluginAssemblyCatalog Create(
            string pluginDirectory,
            string? sharedAssemblyDirectory = null
        )
        {
            var absolutePluginDirectory = System.IO.Path.GetFullPath(pluginDirectory);
            var pluginAssemblies = GetManagedAssemblies(absolutePluginDirectory);
            var managedAssemblies = pluginAssemblies;
            if (!string.IsNullOrWhiteSpace(sharedAssemblyDirectory))
            {
                var absoluteSharedAssemblyDirectory = System.IO.Path.GetFullPath(
                    sharedAssemblyDirectory
                );
                if (
                    Directory.Exists(absoluteSharedAssemblyDirectory)
                    && !string.Equals(
                        absolutePluginDirectory,
                        absoluteSharedAssemblyDirectory,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    managedAssemblies =
                    [
                        .. pluginAssemblies,
                        .. GetManagedAssemblies(absoluteSharedAssemblyDirectory),
                    ];
                }
            }

            managedAssemblies = managedAssemblies
                .OrderBy(metadata => metadata.Path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var files = managedAssemblies
                .GroupBy(metadata => metadata.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<ManagedAssemblyMetadata>)[.. group],
                    StringComparer.OrdinalIgnoreCase
                );

            var plugins = new Dictionary<string, ManagedAssemblyMetadata>(
                StringComparer.OrdinalIgnoreCase
            );
            var duplicatePlugins = new List<DuplicatePluginAssembly>();
            foreach (
                var pluginGroup in pluginAssemblies
                    .Where(metadata =>
                        System
                            .IO.Path.GetFileNameWithoutExtension(metadata.Path)
                            .EndsWith("Plugin", StringComparison.OrdinalIgnoreCase)
                    )
                    .GroupBy(metadata => metadata.Name, StringComparer.OrdinalIgnoreCase)
            )
            {
                var identities = pluginGroup
                    .Select(metadata => metadata.Identity.FullName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (identities.Count != 1)
                {
                    throw CreatePluginConflictException(pluginGroup.Key, pluginGroup);
                }

                var moduleVersionIds = pluginGroup
                    .Select(metadata => metadata.ModuleVersionId)
                    .Distinct()
                    .ToList();
                if (moduleVersionIds.Count != 1)
                {
                    throw CreatePluginConflictException(pluginGroup.Key, pluginGroup);
                }

                var candidates = pluginGroup
                    .OrderBy(metadata => metadata.Path, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var canonicalPlugin = candidates[0];
                plugins.Add(pluginGroup.Key, canonicalPlugin);
                if (candidates.Count > 1)
                {
                    duplicatePlugins.Add(
                        new DuplicatePluginAssembly(
                            pluginGroup.Key,
                            canonicalPlugin.Path,
                            [.. candidates.Skip(1).Select(metadata => metadata.Path)]
                        )
                    );
                }
            }

            return new PluginAssemblyCatalog(files, plugins, duplicatePlugins);
        }

        public IReadOnlyList<ManagedAssemblyMetadata> GetAssemblyCandidates(string assemblyName)
        {
            return _files.TryGetValue(assemblyName, out var candidates) ? candidates : [];
        }

        public bool TryGetPlugin(string assemblyName, out ManagedAssemblyMetadata metadata)
        {
            return _plugins.TryGetValue(assemblyName, out metadata!);
        }

        private static ManagedAssemblyMetadata? TryReadMetadata(string assemblyPath)
        {
            try
            {
                using var stream = File.OpenRead(assemblyPath);
                using var peReader = new PEReader(stream);
                if (!peReader.HasMetadata)
                    return null;

                var metadataReader = peReader.GetMetadataReader();
                if (!metadataReader.IsAssembly)
                    return null;

                var referencedAssemblyNames = metadataReader
                    .AssemblyReferences.Select(referenceHandle =>
                    {
                        var reference = metadataReader.GetAssemblyReference(referenceHandle);
                        return metadataReader.GetString(reference.Name);
                    })
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var moduleDefinition = metadataReader.GetModuleDefinition();

                return new ManagedAssemblyMetadata(
                    System.IO.Path.GetFullPath(assemblyPath),
                    AssemblyName.GetAssemblyName(assemblyPath),
                    metadataReader.GetGuid(moduleDefinition.Mvid),
                    referencedAssemblyNames
                );
            }
            catch (BadImageFormatException)
            {
                return null;
            }
        }

        private static List<ManagedAssemblyMetadata> GetManagedAssemblies(string directory)
        {
            return Directory
                .EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)
                .Select(TryReadMetadata)
                .OfType<ManagedAssemblyMetadata>()
                .ToList();
        }

        private static InvalidOperationException CreatePluginConflictException(
            string assemblyName,
            IEnumerable<ManagedAssemblyMetadata> candidates
        )
        {
            var descriptions = candidates.Select(metadata =>
                $"{metadata.Identity.FullName}, MVID={metadata.ModuleVersionId}, 路径={metadata.Path}"
            );
            return new InvalidOperationException(
                $"插件程序集 {assemblyName} 存在不兼容的重复副本:{Environment.NewLine}{string.Join(Environment.NewLine, descriptions)}"
            );
        }
    }
}
