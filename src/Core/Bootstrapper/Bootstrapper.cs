namespace EagleEye.Core.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;
    using SimpleInjector;

    public static class Bootstrapper
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static IEnumerable<IEagleEyePlugin> FindAvailablePlugins()
        {
            var container = new Container();
            var pluginAssemblies = PluginLocator.FindPluginAssemblies(Path.Combine(AppDomain.CurrentDomain.BaseDirectory));

            container.RegisterPackages(pluginAssemblies);

            try
            {
                return container.GetAllInstances<IEagleEyePlugin>();
            }
            catch (ActivationException e)
            {
                Logger.Warn(e, () => $"Could not find plugins due to an ActivationException. {e.Message}");
                return Enumerable.Empty<IEagleEyePlugin>();
            }
        }

        public static void Bootstrap([NotNull] Container container, IEnumerable<IEagleEyePlugin> plugins)
        {
            Guard.NotNull(container, nameof(container));

            RegisterCore(container);

            RegisterPlugins(container, plugins);

            RegisterReadModels(container);

            // wip
        }

        private static void RegisterCore(Container container)
        {
            container.RegisterInstance<IDateTimeService>(SystemDateTimeService.Instance);
            container.RegisterInstance<IFileService>(SystemFileService.Instance);

            container.RegisterSingleton<IPhotoMimeTypeProvider, MimeTypeProvider>();
        }

        private static void RegisterReadModels([NotNull] Container container)
        {
            DebugGuard.NotNull(container, nameof(container));
        }

        private static void RegisterPlugins([NotNull] Container container, [NotNull] IEnumerable<IEagleEyePlugin> plugins)
        {
            DebugGuard.NotNull(container, nameof(container));
            DebugGuard.NotNull(plugins, nameof(plugins));

            foreach (var plugin in plugins)
            {
                plugin.EnablePlugin(container);
            }
        }
    }
}
