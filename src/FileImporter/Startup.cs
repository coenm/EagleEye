namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Core.Domain.EventStore;
    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Messages;
    using CQRSlite.Queries;
    using CQRSlite.Routing;
    using EagleEye.Core.Domain;
    using EagleEye.Core.Domain.CommandHandlers;
    using EagleEye.Core.ReadModel;
    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EventHandlers;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.JsonSimilarity;
    using EagleEye.FileImporter.Infrastructure.PersistentSerializer;
    using EagleEye.FileImporter.Similarity;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using SearchEngine.Lucene.Bootstrap;
    using SimpleInjector;

    public static class Startup
    {
        public static void ConfigureContainer([NotNull] Container container, [NotNull] string indexFilename)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(indexFilename, nameof(indexFilename));

            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

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

//            container.RegisterSingleton<IEventStore, InMemoryEventStore>();
            container.RegisterSingleton<IEventStore>(() =>
            {
                string basePath = Path.Combine(userDir, "EagleEyeEvents");
                return new FileBasedEventStore(container.GetInstance<IEventPublisher>(), basePath);
            });
            container.RegisterSingleton<ICache, MemoryCache>();

            // add scoped?!
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton); // Repository has two public constructors (why??)
            container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            container.Register<ISession, Session>(Lifestyle.Singleton); // check.

            container.Register<IReadModelFacade, ReadModel>();

            // Scan and register command handlers and event handlers
            var coreAssembly = typeof(MediaItemCommandHandlers).Assembly;
            container.Register(typeof(IHandler<>), coreAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableHandler<>), coreAssembly, Lifestyle.Transient);
            container.Register(typeof(ICommandHandler<>), coreAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableCommandHandler<>), coreAssembly, Lifestyle.Transient);
            container.Register(typeof(IQueryHandler<,>), coreAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableQueryHandler<,>), coreAssembly, Lifestyle.Transient);

            // entity framework stuff??! transient? singleton? ..
            // wip
            container.Register<IEagleEyeRepository, EntityFrameworkEagleEyeRepository>();

            container.Collection.Register(typeof(IDbContextOptionsStrategy), coreAssembly);
            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);

            var fullFile = Path.Combine(userDir, "EagleEye.db");
            string connectionString = $"Filename={fullFile}";

            container.Register<DbContextOptions<EagleEyeDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString));

            container.Register<IEagleEyeDbContextFactory>(
                () =>
                {
                    // arghhh... todo
                    var result = container.GetInstance<EagleEyeDbContextFactory>();
                    result.Initialize().GetAwaiter().GetResult();
                    return result;
                }, Lifestyle.Singleton);

            RegisterSearchEngine(container);

            // strange stuff..
            container.Register<MediaItemConsistency>();
            container.Register<MediaItemCommandHandlers>();
            var registrar = new RouteRegistrar(container);
            registrar.RegisterHandlers(typeof(MediaItemCommandHandlers));
            registrar.RegisterHandlers(typeof(MediaItemConsistency));
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        private static void RegisterSearchEngine([NotNull] Container container)
        {
            DebugGuard.NotNull(container, nameof(container));
            SearchEngineLuceneBootstrapper.Bootstrap(container);
        }
    }
}
