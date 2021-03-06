﻿namespace EagleEye.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

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
    using Nito.AsyncEx;
    using NLog;
    using SimpleInjector;

    public sealed class Bootstrapper
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
            using var container = new Container();

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
            [NotNull] IEnumerable<IEagleEyePlugin> plugins,
            [CanBeNull] IReadOnlyDictionary<string, object> config,
            [CanBeNull] string connectionStringEventStore = null)
        {
            Guard.Argument(plugins, nameof(plugins)).NotNull();

            var bootstrapper = new Bootstrapper();

            bootstrapper.RegisterCore(connectionStringEventStore);

            bootstrapper.RegisterPlugins(plugins.ToArray(), config);

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

        /// <param name="indexBaseDirectory">Base directory for the Lucene index files. <c>null</c> Or an empty string will result in an InMemory index.</param>
        public void RegisterSearchEngineReadModel([CanBeNull] string indexBaseDirectory)
        {
            if (readModelSearchEngineEnabled)
                return;

            Photo.ReadModel.SearchEngineLucene.Bootstrapper.BootstrapSearchEngineLuceneReadModel(
                container,
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

        /// <param name="connectionString">Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty. Should start with 'InMemory' or with 'Filename='.</param>
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

        private void RegisterCore([CanBeNull] string connectionStringEventStore)
        {
            container.RegisterInstance<IDateTimeService>(SystemDateTimeService.Instance);
            container.RegisterInstance<IFileService>(SystemFileService.Instance); // RelativeSystemFileService
            container.RegisterInstance<IDirectoryService>(SystemDirectoryService.Instance);

            container.RegisterSingleton<IPhotoMimeTypeProvider, MimeTypeProvider>();
            container.RegisterSingleton<IFileSha256HashProvider, FileSha256HashProvider>();

            RegisterCqrsLite();
            RegisterEventStore(connectionStringEventStore);

            RegisterPhotoDomain();

            container.Collection.Append<IEagleEyeProcess, DummyProcess>(Lifestyle.Singleton);
            container.Collection.Append<IEagleEyeInitialize, DummyEagleEyeInitialize>(Lifestyle.Singleton);
        }

        private void RegisterCqrsLite()
        {
            container.Register<Router>(Lifestyle.Singleton);
            container.Register<ICommandSender>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IEventPublisher>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IHandlerRegistrar>(container.GetInstance<Router>, Lifestyle.Singleton);
            // container.RegisterDecorator<ICommandSender, RetryCommandSenderDecorator>(Lifestyle.Singleton);

            container.RegisterSingleton<ICache, MemoryCache>();

            // add scoped?!

            // Repository has two public constructors.
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton);
            // container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            container.RegisterDecorator<IRepository, ConcurrencyRepositoryDecorator>(Lifestyle.Singleton);

            container.Register<ISession, Session>(Lifestyle.Singleton);
            container.RegisterDecorator<ISession, ConcurrencySessionDecorator>(Lifestyle.Singleton);
        }

        private void RegisterEventStore([CanBeNull] string connectionString)
        {
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
            {
                // same instance (https://simpleinjector.readthedocs.io/en/latest/howto.html#register-multiple-interfaces-with-the-same-implementation)
                container.Register<INEventStoreAdapterFactory, NEventStoreAdapterInMemoryFactory>(Lifestyle.Singleton);
                container.Register<INEventStoreEventExporterAdapterFactory, NEventStoreAdapterInMemoryFactory>(Lifestyle.Singleton);
            }
            else
            {
                // not sure if this is the way to do this.
                container.Register<NEventStoreAdapterSqliteFactory>(() => new NEventStoreAdapterSqliteFactory(connectionString), Lifestyle.Singleton);
                container.Register<INEventStoreAdapterFactory>(() => container.GetInstance<NEventStoreAdapterSqliteFactory>(), Lifestyle.Singleton);
                container.Register<INEventStoreEventExporterAdapterFactory>(() => container.GetInstance<NEventStoreAdapterSqliteFactory>(), Lifestyle.Singleton);
            }

            container.Register<IEventStore>(
                                            () =>
                                            {
                                                // ReSharper disable once ConvertToLambdaExpression
                                                return container.GetInstance<INEventStoreAdapterFactory>()
                                                                .Create(container.GetInstance<IEventPublisher>());
                                            },
                                            Lifestyle.Singleton);
            container.RegisterDecorator<IEventStore, ConcurrencyEventStoreDecorator>(Lifestyle.Singleton);

            container.Register<IEventExporter>(() => container.GetInstance<INEventStoreEventExporterAdapterFactory>().Create(), Lifestyle.Singleton);
        }

        private void RegisterPhotoDomain()
        {
            Guard.Argument(container, nameof(container)).NotNull();

            Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        private void RegisterPlugins([NotNull] IEagleEyePlugin[] plugins, [CanBeNull] IReadOnlyDictionary<string, object> config)
        {
            Guard.Argument(plugins, nameof(plugins)).NotNull();

            foreach (var plugin in plugins)
            {
                plugin?.EnablePlugin(container, config);
            }
        }
    }

    internal class ConcurrencySessionDecorator : ISession
    {
        private readonly ISession decoratee;
        private readonly AsyncLock mutex = new AsyncLock();

        public ConcurrencySessionDecorator(ISession decoratee)
        {
            this.decoratee = decoratee;
        }

        public async Task Add<T>(T aggregate, CancellationToken cancellationToken = new CancellationToken())
            where T : AggregateRoot
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
                await decoratee.Add(aggregate, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> Get<T>(Guid id, int? expectedVersion = null, CancellationToken cancellationToken = new CancellationToken())
            where T : AggregateRoot
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
                return await decoratee.Get<T>(id, expectedVersion, cancellationToken).ConfigureAwait(false);
        }

        public async Task Commit(CancellationToken cancellationToken = new CancellationToken())
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
                await decoratee.Commit(cancellationToken).ConfigureAwait(false);
        }
    }

    internal class ConcurrencyRepositoryDecorator : IRepository
    {
        private readonly IRepository decoratee;
        private readonly AsyncLock mutex = new AsyncLock();

        public ConcurrencyRepositoryDecorator(IRepository decoratee)
        {
            this.decoratee = decoratee;
        }

        public async Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = new CancellationToken())
            where T : AggregateRoot
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
                await decoratee.Save(aggregate, expectedVersion, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = new CancellationToken())
            where T : AggregateRoot
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
                return await decoratee.Get<T>(aggregateId, cancellationToken).ConfigureAwait(false);
        }
    }

    internal class ConcurrencyEventStoreDecorator : IEventStore
    {
        private readonly IEventStore decoratee;
        private readonly AsyncLock mutex = new AsyncLock();

        public ConcurrencyEventStoreDecorator([NotNull] IEventStore decoratee)
        {
            this.decoratee = decoratee;
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = new CancellationToken())
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                await decoratee.Save(events, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = new CancellationToken())
        {
            using (await mutex.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                return await decoratee.Get(aggregateId, fromVersion, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    internal class DummyProcess : IEagleEyeProcess
    {
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }

    internal class DummyEagleEyeInitialize : IEagleEyeInitialize
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
