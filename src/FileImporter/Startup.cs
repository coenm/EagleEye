namespace EagleEye.FileImporter
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
    using EagleEye.Core.DefaultImplementations.EventStore;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.JsonSimilarity;
    using EagleEye.FileImporter.Infrastructure.PersistentSerializer;
    using EagleEye.FileImporter.Similarity;
    using EagleEye.Photo.ReadModel.SearchEngineLucene;
    using Helpers.Guards; using Dawn;
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
        public static void ConfigureContainer(
            [NotNull] Container container,
            [NotNull] string indexFilename,
            [NotNull] string connectionStringHangFire)
        {
            Helpers.Guards.Guard.NotNull(container, nameof(container));
            Helpers.Guards.Guard.NotNullOrWhiteSpace(indexFilename, nameof(indexFilename));
            Helpers.Guards.Guard.NotNullOrWhiteSpace(connectionStringHangFire, nameof(connectionStringHangFire));

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

            // container.RegisterSingleton<IEventStore, InMemoryEventStore>();
            container.RegisterSingleton<IEventStore>(() =>
            {
                string basePath = Path.Combine(userDir, "Events");
                return new FileBasedEventStore(container.GetInstance<IEventPublisher>(), basePath);
            });
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

        public static string CreateFullFilename([NotNull] string filename)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(filename, nameof(filename));
            return Path.Combine(GetUserDirectory(), filename);
        }

        public static string CreateSqlLiteFileConnectionString([NotNull] string fullFilename)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(fullFilename, nameof(fullFilename));
            return $"Filename={fullFilename};";
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            Helpers.Guards.Guard.NotNull(container, nameof(container));
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        public static async Task InitializeAllServices([NotNull] Container container)
        {
            Helpers.Guards.Guard.NotNull(container, nameof(container));
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
            DebugHelpers.Guards.Guard.NotNull(container, nameof(container));
            DebugHelpers.Guards.Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            DebugHelpers.Guards.Guard.NotNullOrWhiteSpace(connectionStringHangFire, nameof(connectionStringHangFire));

            global::EagleEye.Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(container, connectionString, connectionStringHangFire);
        }

        private static void RegisterPhotoDomain([NotNull] Container container)
        {
            DebugHelpers.Guards.Guard.NotNull(container, nameof(container));

            EagleEye.Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        private static void RegisterPhotoDatabaseReadModel([NotNull] Container container, [CanBeNull] string connectionString)
        {
            DebugHelpers.Guards.Guard.NotNull(container, nameof(container));
            DebugHelpers.Guards.Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            global::EagleEye.Photo.ReadModel.EntityFramework.Bootstrapper.BootstrapEntityFrameworkReadModel(
                                                           container,
                                                           connectionString);
        }

        private static void RegisterSearchEngineReadModel([NotNull] Container container, [CanBeNull] string indexBaseDirectory)
        {
            DebugHelpers.Guards.Guard.NotNull(container, nameof(container));

            Bootstrapper.BootstrapSearchEngineLuceneReadModel(
                container,
                string.IsNullOrWhiteSpace(indexBaseDirectory),
                indexBaseDirectory);
        }
    }
}
