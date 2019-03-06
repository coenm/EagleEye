namespace EagleEye.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Routing;
    using EagleEye.Core.DefaultImplementations;
    using EagleEye.Core.DefaultImplementations.EventStore;
    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;
    using SimpleInjector;

    public class Bootstrapper
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        [NotNull] private readonly Container container;

        private Bootstrapper()
        {
            container = new Container();
        }

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

        public static Bootstrapper Initialize(
            [NotNull] string baseDirectory,
            [NotNull] IEnumerable<IEagleEyePlugin> plugins)
        {
            Guard.NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));
            Guard.NotNull(plugins, nameof(plugins));

            var bootstrapper = new Bootstrapper();

            bootstrapper.RegisterCore(baseDirectory);

            bootstrapper.RegisterPlugins(plugins);

            return bootstrapper;
        }

        public Container Finalize()
        {
            return container;
        }

        private void RegisterPhotoDomain()
        {
            DebugGuard.NotNull(container, nameof(container));

            Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        public void RegisterSearchEngineReadModel([CanBeNull] string indexBaseDirectory)
        {
            Photo.ReadModel.SearchEngineLucene.Bootstrapper.BootstrapSearchEngineLuceneReadModel(
                container,
                string.IsNullOrWhiteSpace(indexBaseDirectory),
                indexBaseDirectory);
        }

        public void RegisterSimilarityReadModel([NotNull] string connectionString, [NotNull] string connectionStringHangFire)
        {
            DebugGuard.NotNull(container, nameof(container));
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            DebugGuard.NotNullOrWhiteSpace(connectionStringHangFire, nameof(connectionStringHangFire));

            Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(container, connectionString, connectionStringHangFire);
        }

        public void RegisterPhotoDatabaseReadModel([CanBeNull] string connectionString)
        {
            DebugGuard.NotNull(container, nameof(container));
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            Photo.ReadModel.EntityFramework.Bootstrapper.BootstrapEntityFrameworkReadModel(
                container,
                connectionString);
        }

        private void RegisterCore([NotNull] string baseDirectory)
        {
            DebugGuard.NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));

            container.RegisterInstance<IDateTimeService>(SystemDateTimeService.Instance);
            container.RegisterInstance<IFileService>(SystemFileService.Instance);

            container.RegisterSingleton<IPhotoMimeTypeProvider, MimeTypeProvider>();

            RegisterCqrsLite(baseDirectory);

            RegisterPhotoDomain();
        }

        private void RegisterCqrsLite([NotNull] string baseDirectory)
        {
            DebugGuard.NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));

            container.Register<Router>(Lifestyle.Singleton);
            container.Register<ICommandSender>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IEventPublisher>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IHandlerRegistrar>(container.GetInstance<Router>, Lifestyle.Singleton);

            // container.RegisterSingleton<IEventStore, InMemoryEventStore>();
            container.RegisterSingleton<IEventStore>(() =>
            {
                var basePath = Path.Combine(baseDirectory, "Events");
                return new FileBasedEventStore(container.GetInstance<IEventPublisher>(), basePath);
            });
            container.RegisterSingleton<ICache, MemoryCache>();

            // add scoped?!

            // Repository has two public constructors.
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton);
            container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            container.Register<ISession, Session>(Lifestyle.Singleton); // check.
        }

        private void RegisterPlugins([NotNull] IEnumerable<IEagleEyePlugin> plugins)
        {
            DebugGuard.NotNull(plugins, nameof(plugins));

            foreach (var plugin in plugins)
            {
                plugin.EnablePlugin(container);
            }
        }
    }
}
