using System;
using System.Collections.Generic;
using System.Linq;

namespace UzonMail.Utils.Plugin
{
    /// <summary>
    /// 根据插件程序集引用关系生成稳定的依赖优先顺序。
    /// </summary>
    internal static class PluginDependencySorter
    {
        public static IReadOnlyList<ManagedAssemblyMetadata> Sort(
            IReadOnlyList<ManagedAssemblyMetadata> plugins
        )
        {
            var pluginsByName = plugins.ToDictionary(
                metadata => metadata.Name,
                StringComparer.OrdinalIgnoreCase
            );
            var dependencyNames = plugins.ToDictionary(
                metadata => metadata.Name,
                metadata =>
                    metadata
                        .ReferencedAssemblyNames.Where(referenceName =>
                            !referenceName.Equals(metadata.Name, StringComparison.OrdinalIgnoreCase)
                            && pluginsByName.ContainsKey(referenceName)
                        )
                        .ToHashSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase
            );
            var dependentNames = plugins.ToDictionary(
                metadata => metadata.Name,
                _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var (pluginName, dependencies) in dependencyNames)
            {
                foreach (var dependencyName in dependencies)
                {
                    dependentNames[dependencyName].Add(pluginName);
                }
            }

            var remainingDependencyCounts = dependencyNames.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Count,
                StringComparer.OrdinalIgnoreCase
            );
            var readyPluginNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (
                var pluginName in remainingDependencyCounts
                    .Where(pair => pair.Value == 0)
                    .Select(pair => pair.Key)
            )
            {
                readyPluginNames.Add(pluginName);
            }

            var sortedPlugins = new List<ManagedAssemblyMetadata>(plugins.Count);
            while (readyPluginNames.Count > 0)
            {
                var pluginName = readyPluginNames.Min!;
                readyPluginNames.Remove(pluginName);
                sortedPlugins.Add(pluginsByName[pluginName]);

                foreach (var dependentName in dependentNames[pluginName])
                {
                    remainingDependencyCounts[dependentName]--;
                    if (remainingDependencyCounts[dependentName] == 0)
                    {
                        readyPluginNames.Add(dependentName);
                    }
                }
            }

            if (sortedPlugins.Count == plugins.Count)
                return sortedPlugins;

            var cycle = FindCycle(dependencyNames);
            throw new InvalidOperationException($"检测到插件循环依赖: {string.Join(" -> ", cycle)}");
        }

        private static IReadOnlyList<string> FindCycle(
            IReadOnlyDictionary<string, HashSet<string>> dependencyNames
        )
        {
            var states = dependencyNames.Keys.ToDictionary(
                pluginName => pluginName,
                _ => 0,
                StringComparer.OrdinalIgnoreCase
            );
            var path = new List<string>();
            foreach (var pluginName in dependencyNames.Keys)
            {
                var cycle = Visit(pluginName, dependencyNames, states, path);
                if (cycle.Count > 0)
                    return cycle;
            }

            return [];
        }

        private static IReadOnlyList<string> Visit(
            string pluginName,
            IReadOnlyDictionary<string, HashSet<string>> dependencyNames,
            IDictionary<string, int> states,
            List<string> path
        )
        {
            if (states[pluginName] == 2)
                return [];
            if (states[pluginName] == 1)
            {
                var cycleStart = path.FindIndex(name =>
                    name.Equals(pluginName, StringComparison.OrdinalIgnoreCase)
                );
                return [.. path.Skip(cycleStart), pluginName];
            }

            states[pluginName] = 1;
            path.Add(pluginName);
            foreach (var dependencyName in dependencyNames[pluginName])
            {
                var cycle = Visit(dependencyName, dependencyNames, states, path);
                if (cycle.Count > 0)
                    return cycle;
            }

            path.RemoveAt(path.Count - 1);
            states[pluginName] = 2;
            return [];
        }
    }

    internal sealed record LoadedPlugin(ManagedAssemblyMetadata Assembly, IPlugin Instance);

    /// <summary>
    /// 在满足程序集依赖的前提下，按插件优先级生成配置执行顺序。
    /// </summary>
    internal static class PluginExecutionSorter
    {
        public static IReadOnlyList<LoadedPlugin> Sort(IReadOnlyList<LoadedPlugin> plugins)
        {
            var pluginIndexesByAssembly = plugins
                .Select((plugin, index) => new { plugin.Assembly.Name, Index = index })
                .GroupBy(pair => pair.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(pair => pair.Index).ToList(),
                    StringComparer.OrdinalIgnoreCase
                );
            var remainingDependencyCounts = new int[plugins.Count];
            var dependentIndexes = Enumerable
                .Range(0, plugins.Count)
                .Select(_ => new List<int>())
                .ToArray();

            for (var pluginIndex = 0; pluginIndex < plugins.Count; pluginIndex++)
            {
                foreach (
                    var dependencyName in plugins[pluginIndex].Assembly.ReferencedAssemblyNames
                )
                {
                    if (!pluginIndexesByAssembly.TryGetValue(dependencyName, out var dependencies))
                        continue;

                    foreach (var dependencyIndex in dependencies)
                    {
                        dependentIndexes[dependencyIndex].Add(pluginIndex);
                        remainingDependencyCounts[pluginIndex]++;
                    }
                }
            }

            var readyIndexes = new SortedSet<int>(
                Comparer<int>.Create((left, right) => ComparePlugins(plugins, left, right))
            );
            for (var pluginIndex = 0; pluginIndex < plugins.Count; pluginIndex++)
            {
                if (remainingDependencyCounts[pluginIndex] == 0)
                    readyIndexes.Add(pluginIndex);
            }

            var sortedPlugins = new List<LoadedPlugin>(plugins.Count);
            while (readyIndexes.Count > 0)
            {
                var pluginIndex = readyIndexes.Min;
                readyIndexes.Remove(pluginIndex);
                sortedPlugins.Add(plugins[pluginIndex]);
                foreach (var dependentIndex in dependentIndexes[pluginIndex])
                {
                    remainingDependencyCounts[dependentIndex]--;
                    if (remainingDependencyCounts[dependentIndex] == 0)
                        readyIndexes.Add(dependentIndex);
                }
            }

            if (sortedPlugins.Count != plugins.Count)
                throw new InvalidOperationException("插件配置顺序包含无法满足的循环依赖。");

            return sortedPlugins;
        }

        private static int ComparePlugins(
            IReadOnlyList<LoadedPlugin> plugins,
            int leftIndex,
            int rightIndex
        )
        {
            if (leftIndex == rightIndex)
                return 0;

            var left = plugins[leftIndex];
            var right = plugins[rightIndex];
            var priorityComparison = left.Instance.Priority.CompareTo(right.Instance.Priority);
            if (priorityComparison != 0)
                return priorityComparison;

            var typeNameComparison = StringComparer.Ordinal.Compare(
                left.Instance.GetType().FullName,
                right.Instance.GetType().FullName
            );
            return typeNameComparison != 0 ? typeNameComparison : leftIndex.CompareTo(rightIndex);
        }
    }
}
