namespace EagleEye.Bootstrap
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;

    public static class PluginLocator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [NotNull]
        public static IEnumerable<Assembly> FindPluginAssemblies([NotNull] string baseDirectory)
        {
            Guard.NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));

            Logger.Debug(() => $"Plugin base directory {baseDirectory}");

            var assemblies = GetPluginAssembliesInDirectory(baseDirectory);

            foreach (var dir in GetPluginDirectories(baseDirectory))
            {
                assemblies = assemblies.Concat(GetPluginAssembliesInDirectory(dir));
            }

            return assemblies;
        }

        private static IEnumerable<string> GetPluginDirectories(string baseDirectory)
        {
            var pluginBaseDirectory = Path.Combine(baseDirectory, "Plugins");

            if (!Directory.Exists(pluginBaseDirectory))
                return Enumerable.Empty<string>();

            return Directory.EnumerateDirectories(pluginBaseDirectory);
        }

        [NotNull]
        private static IEnumerable<Assembly> GetPluginAssembliesInDirectory([NotNull] string baseDirectory)
        {
            DebugGuard.NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));

            return new DirectoryInfo(baseDirectory)
                .GetFiles()
                .Where(file =>
                    file.Name.StartsWith("EagleEye.Plugin.")
                    &&
                    file.Extension.ToLower() == ".dll")
                .Select(file => Assembly.Load(AssemblyName.GetAssemblyName(file.FullName)));
        }
    }
}
