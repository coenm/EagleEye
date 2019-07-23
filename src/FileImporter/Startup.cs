﻿namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Routing;
    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.EventStore.NEventStoreAdapter;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.JsonSimilarity;
    using EagleEye.FileImporter.Infrastructure.PersistentSerializer;
    using EagleEye.FileImporter.Similarity;
    using EagleEye.Photo.ReadModel.SearchEngineLucene;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Startup
    {
        /// <summary>
        /// Bootstrap the application by setting up the Dependency Container.
        /// </summary>
        /// <param name="container">Dependency container</param>
        /// <param name="indexFilename">(obsolete)</param>
        /// <param name="connectionStringHangFire">ConnectionString for HangFire Similarity Database.</param>
        /// <param name="filenameEventStore">Filename sqlite eventstore.</param>
        public static void ConfigureContainer(
            [NotNull] Container container,
            [NotNull] string indexFilename,
            [NotNull] string connectionStringHangFire,
            [CanBeNull] string filenameEventStore)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(indexFilename, nameof(indexFilename)).NotNull().NotWhiteSpace();
            Guard.Argument(connectionStringHangFire, nameof(connectionStringHangFire)).NotNull().NotWhiteSpace();

            string userDir = GetUserDirectory();

            var similarityFilename = indexFilename + ".similarity.json";
            // todo check arguments.
            // container.RegisterSingleton<IContentResolver>(new RelativeFilesystemContentResolver(rootPath));
            container.RegisterInstance<IContentResolver>(FilesystemContentResolver.Instance);
            container.RegisterSingleton<IImageDataRepository>(() => new SingleImageDataRepository(new JsonToFileSerializer<List<ImageData>>(indexFilename)));
            container.RegisterSingleton<ISimilarityRepository>(() => new SingleFileSimilarityRepository(new JsonToFileSerializer<List<SimilarityResultStorage>>(similarityFilename)));

            // CQRS lite stuff.
            container.Register<Router>(Lifestyle.Singleton);
            container.Register<ICommandSender>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IEventPublisher>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IHandlerRegistrar>(container.GetInstance<Router>, Lifestyle.Singleton);

            RegisterEventStore(container, userDir, filenameEventStore);
            container.RegisterSingleton<ICache, MemoryCache>();

            // add scoped?!
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton); // Repository has two public constructors (why??)
            container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            container.Register<ISession, Session>(Lifestyle.Singleton); // check.

            RegisterPhotoDomain(container);

            RegisterSearchEngineReadModel(container, Path.Combine(userDir, "Index"));

            var connectionString1 = CreateSqlLiteFileConnectionString(CreateFullFilename("EagleEye.db"));
            RegisterPhotoDatabaseReadModel(container, connectionString1);

            var connectionStringSimilarity = CreateSqlLiteFileConnectionString(CreateFullFilename("Similarity.db"));
            var connectionStringHangfire1 = CreateSqlLiteFileConnectionString(CreateFullFilename("Similarity.Hangfire.db"));
            RegisterSimilarityReadModel(container, connectionStringSimilarity, connectionStringHangFire);

            // strange stuff..
            var registrar = new RouteRegistrar(container);
            registrar.RegisterHandlers(EagleEye.Photo.Domain.Bootstrapper.GetEventHandlerTypesPhotoDomain());
            registrar.RegisterHandlers(global::EagleEye.Photo.ReadModel.EntityFramework.Bootstrapper.GetEventHandlerTypes());
            registrar.RegisterHandlers(Bootstrapper.GetEventHandlerTypes());
            registrar.RegisterHandlers(global::EagleEye.Photo.ReadModel.Similarity.Bootstrapper.GetEventHandlerTypes());
        }

        private static void RegisterEventStore(Container container, string baseDirectory, [CanBeNull] string connectionString)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(baseDirectory, nameof(baseDirectory)).NotNull().NotWhiteSpace();

            /*
            // InMemory
            // container.RegisterSingleton<IEventStore, InMemoryEventStore>();
            */

            /*
            // File Based
            container.RegisterSingleton<IEventStore>(
                () =>
                {
                    var basePath = Path.Combine(baseDirectory, "Events");
                    return new FileBasedEventStore(container.GetInstance<IEventPublisher>(), basePath);
                });
            */

            // Use NEventStore
            // container.Register<INEventStoreAdapterFactory, NEventStoreAdapterInMemoryFactory>(Lifestyle.Singleton);
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


        public static string CreateFullFilename([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();
            return Path.Combine(GetUserDirectory(), filename);
        }

        public static string CreateSqlLiteFileConnectionString([NotNull] string fullFilename)
        {
            Guard.Argument(fullFilename, nameof(fullFilename)).NotNull().NotWhiteSpace();
            return $"Filename={fullFilename};";
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        public static async Task InitializeAllServices([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();
            await Task.WhenAll(instancesToInitialize.Select(instance => instance.InitializeAsync())).ConfigureAwait(false);
        }

        public static void StartServices([NotNull] Container container)
        {
            var allInstances = container.GetAllInstances<IEagleEyeProcess>();
            foreach (var eagleEyeProcess in allInstances)
            {
                eagleEyeProcess.Start();
            }
        }

        public static void StopServices([NotNull] Container container)
        {
            var allEagleEyeProcesses = container.GetAllInstances<IEagleEyeProcess>();
            foreach (var eagleEyeProcess in allEagleEyeProcesses)
            {
                eagleEyeProcess.Stop();
            }
        }

        private static string GetUserDirectory()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userDir, "EagleEye");
        }

        private static void RegisterSimilarityReadModel([NotNull] Container container, [NotNull] string connectionString, [NotNull] string connectionStringHangFire)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace();
            Guard.Argument(connectionStringHangFire, nameof(connectionStringHangFire)).NotNull().NotWhiteSpace();

            global::EagleEye.Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(container, connectionString, connectionStringHangFire);
        }

        private static void RegisterPhotoDomain([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            EagleEye.Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        private static void RegisterPhotoDatabaseReadModel([NotNull] Container container, [CanBeNull] string connectionString)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace();

            global::EagleEye.Photo.ReadModel.EntityFramework.Bootstrapper.BootstrapEntityFrameworkReadModel(
                                                           container,
                                                           connectionString);
        }

        private static void RegisterSearchEngineReadModel([NotNull] Container container, [CanBeNull] string indexBaseDirectory)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            Bootstrapper.BootstrapSearchEngineLuceneReadModel(
                container,
                string.IsNullOrWhiteSpace(indexBaseDirectory),
                indexBaseDirectory);
        }
    }
}
