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
        private bool readModelSearchEngineEnabled;
        private bool readModelSimilarityEnabled;
        private bool readModelDatabaseEnabled;

        private Bootstrapper()
        {
            container = new Container();
        }

        public static IEnumerable<IEagleEyePlugin> FindAvailablePlugins()
        {
            using (var container = new Container())
            {
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
            // make sure that the CqrsLite lib has knowledge how to create the event handlers.
            // in the feature, this should be different.
            var registrar = new RouteRegistrar(container);

            registrar.RegisterHandlers(Photo.Domain.Bootstrapper.GetEventHandlerTypesPhotoDomain());

            if (readModelSearchEngineEnabled)
                registrar.RegisterHandlers(Photo.ReadModel.SearchEngineLucene.Bootstrapper.GetEventHandlerTypes());

            if (readModelDatabaseEnabled)
                registrar.RegisterHandlers(Photo.ReadModel.EntityFramework.Bootstrapper.GetEventHandlerTypes());

            if (readModelSimilarityEnabled)
                registrar.RegisterHandlers(Photo.ReadModel.Similarity.Bootstrapper.GetEventHandlerTypes());

            return container;
        }

        public void RegisterSearchEngineReadModel([CanBeNull] string indexBaseDirectory)
        {
            DebugGuard.NotNullOrWhiteSpace(indexBaseDirectory, nameof(indexBaseDirectory));

            if (readModelSearchEngineEnabled)
                return;

            Photo.ReadModel.SearchEngineLucene.Bootstrapper.BootstrapSearchEngineLuceneReadModel(
                container,
                string.IsNullOrWhiteSpace(indexBaseDirectory),
                indexBaseDirectory);

            readModelSearchEngineEnabled = true;
        }

        public void RegisterSimilarityReadModel([NotNull] string connectionString, [NotNull] string connectionStringHangFire)
        {
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            DebugGuard.NotNullOrWhiteSpace(connectionStringHangFire, nameof(connectionStringHangFire));

            if (readModelSimilarityEnabled)
                return;

            Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(container, connectionString, connectionStringHangFire);

            readModelSimilarityEnabled = true;
        }

        public void RegisterPhotoDatabaseReadModel([CanBeNull] string connectionString)
        {
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            if (readModelDatabaseEnabled)
                return;

            Photo.ReadModel.EntityFramework.Bootstrapper.BootstrapEntityFrameworkReadModel(
                container,
                connectionString);

            readModelDatabaseEnabled = true;
        }

        public void AddRegistrations(Action<Container> action)
        {
            action?.Invoke(container);
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
            container.Register<ISession, Session>(Lifestyle.Singleton);
        }

        private void RegisterPhotoDomain()
        {
            DebugGuard.NotNull(container, nameof(container));

            Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        private void RegisterPlugins([NotNull] IEnumerable<IEagleEyePlugin> plugins)
        {
            DebugGuard.NotNull(plugins, nameof(plugins));

            foreach (var plugin in plugins)
            {
                plugin?.EnablePlugin(container);
            }
        }
    }
}
