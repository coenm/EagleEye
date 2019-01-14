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

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.Core.Interfaces;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.JsonSimilarity;
    using EagleEye.FileImporter.Infrastructure.PersistentSerializer;
    using EagleEye.FileImporter.Similarity;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Startup
    {
        public static void ConfigureContainer([NotNull] Container container, [NotNull] string indexFilename)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(indexFilename, nameof(indexFilename));

            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            userDir = Path.Combine(userDir, "EagleEye");

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

            var fullFileEagleEye = Path.Combine(userDir, "EagleEye.db");
            string connectionString1 = $"Filename={fullFileEagleEye}";
            RegisterPhotoDatabaseReadModel(container, connectionString1);

            var fullFileSimilarity = Path.Combine(userDir, "Similarity.db");
            string connectionString2 = $"Filename={fullFileSimilarity}";

            var fullFileSimilarityHangfire = Path.Combine(userDir, "Similarity.Hangfire.db");
            string connectionStringHangfire = $"Filename={fullFileSimilarityHangfire};";

            RegisterSimilarityReadModel(container, connectionString2, connectionStringHangfire);

            // strange stuff..
            var registrar = new RouteRegistrar(container);
            registrar.RegisterHandlers(EagleEye.Photo.Domain.Bootstrapper.GetEventHandlerTypesPhotoDomain());
            registrar.RegisterHandlers(global::Photo.EntityFramework.ReadModel.Bootstrapper.GetEventHandlerTypes());
            registrar.RegisterHandlers(SearchEngine.LuceneNet.ReadModel.Bootstrapper.GetEventHandlerTypes());
            registrar.RegisterHandlers(global::Photo.ReadModel.Similarity.Bootstrapper.GetEventHandlerTypes());
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }


        public static async Task InitializeAllServices([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));
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

        private static void RegisterSimilarityReadModel([NotNull] Container container, [NotNull] string connectionString, string connectionstringHangfire)
        {
            DebugGuard.NotNull(container, nameof(container));
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            DebugGuard.NotNullOrWhiteSpace(connectionstringHangfire, nameof(connectionstringHangfire));

            global::Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(container, connectionString, connectionstringHangfire);
        }

        private static void RegisterPhotoDomain([NotNull] Container container)
        {
            DebugGuard.NotNull(container, nameof(container));

            EagleEye.Photo.Domain.Bootstrapper.BootstrapPhotoDomain(container);
        }

        private static void RegisterPhotoDatabaseReadModel([NotNull] Container container, [CanBeNull] string connectionString)
        {
            DebugGuard.NotNull(container, nameof(container));
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            global::Photo.EntityFramework.ReadModel.Bootstrapper.BootstrapEntityFrameworkReadModel(
                                                           container,
                                                           connectionString);
        }

        private static void RegisterSearchEngineReadModel([NotNull] Container container, [CanBeNull] string indexBaseDirectory)
        {
            DebugGuard.NotNull(container, nameof(container));

            SearchEngine.LuceneNet.ReadModel.Bootstrapper.BootstrapSearchEngineLuceneReadModel(
                container,
                string.IsNullOrWhiteSpace(indexBaseDirectory),
                indexBaseDirectory);
        }
    }
}
