﻿namespace EagleEye.Bootstrap
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
    using Dawn;
    using EagleEye.Core.DefaultImplementations;
    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.EventStore.NEventStoreAdapter;
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
            [NotNull] IEnumerable<IEagleEyePlugin> plugins,
            [CanBeNull] string connectionStringEventStore = null)
        {
            Guard.Argument(baseDirectory, nameof(baseDirectory)).NotNull().NotWhiteSpace();
            Guard.Argument(plugins, nameof(plugins)).NotNull();

            var bootstrapper = new Bootstrapper();

            bootstrapper.RegisterCore(baseDirectory, connectionStringEventStore);

            bootstrapper.RegisterPlugins(plugins.ToArray());

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
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace();
            Guard.Argument(connectionStringHangFire, nameof(connectionStringHangFire)).NotNull().NotWhiteSpace();

            if (readModelSimilarityEnabled)
                return;

            Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(container, connectionString, connectionStringHangFire);

            readModelSimilarityEnabled = true;
        }

        public void RegisterPhotoDatabaseReadModel([NotNull] string connectionString)
        {
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace();

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

        private void RegisterCore([NotNull] string baseDirectory, [CanBeNull] string connectionStringEventStore)
        {
            Guard.Argument(baseDirectory, nameof(baseDirectory)).NotNull().NotWhiteSpace();

            container.RegisterInstance<IDateTimeService>(SystemDateTimeService.Instance);
            container.RegisterInstance<IFileService>(SystemFileService.Instance);

            container.RegisterSingleton<IPhotoMimeTypeProvider, MimeTypeProvider>();
            container.RegisterSingleton<IFileSha256HashProvider, FileSha256HashProvider>();

            RegisterCqrsLite();
            RegisterEventStore(baseDirectory, connectionStringEventStore);

            RegisterPhotoDomain();
        }

        private void RegisterCqrsLite()
        {
            container.Register<Router>(Lifestyle.Singleton);
            container.Register<ICommandSender>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IEventPublisher>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IHandlerRegistrar>(container.GetInstance<Router>, Lifestyle.Singleton);

            container.RegisterSingleton<ICache, MemoryCache>();

            // add scoped?!

            // Repository has two public constructors.
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton);
            container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            container.Register<ISession, Session>(Lifestyle.Singleton);
        }

        private void RegisterEventStore(string baseDirectory, [CanBeNull] string connectionString)
        {
            Guard.Argument(baseDirectory, nameof(baseDirectory)).NotNull().NotWhiteSpace();

            /*
                        // InMemory
                        // container.RegisterSingleton<IEventStore, InMemoryEventStore>();
            */

            /*
                        // File Based
                        container.RegisterSingleton<IEventStore>(() =>
                                                                 {
                                                                     var basePath = Path.Combine(baseDirectory, "Events");
                                                                     return new FileBasedEventStore(container.GetInstance<IEventPublisher>(), basePath);
                                                                 });
            */

            // Use NEventStore
            if (string.IsNullOrWhiteSpace(connectionString))
                container.Register<INEventStoreAdapterFactory, NEventStoreAdapterInMemoryFactory>(Lifestyle.Singleton);
            else
                container.Register<INEventStoreAdapterFactory>(() => new NEventStoreAdapterSqliteFactory(connectionString), Lifestyle.Singleton);

            container.Register<IEventStore>(
                                            () =>
                                            {
                                                // ReSharper disable once ConvertToLambdaExpression
                                                return container.GetInstance<INEventStoreAdapterFactory>()
                                                                .Create(container.GetInstance<IEventPublisher>());
                                            },
                                            Lifestyle.Singleton);
        }

        private void RegisterPhotoDomain()
        {
            Guard.Argument(container, nameof(container)).NotNull();

            Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        private void RegisterPlugins([NotNull] IEagleEyePlugin[] plugins)
        {
            Guard.Argument(plugins, nameof(plugins)).NotNull();

            foreach (var plugin in plugins)
            {
                plugin?.EnablePlugin(container);
            }
        }
    }
}
