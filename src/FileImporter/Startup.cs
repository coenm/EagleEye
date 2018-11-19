namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;
    using Core.ReadModel.EntityFramework.SqlLite;
    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Messages;
    using CQRSlite.Routing;
    using EagleEye.Core.Domain;
    using EagleEye.Core.Domain.CommandHandlers;
    using EagleEye.Core.ReadModel;
    using EagleEye.Core.ReadModel.EntityFramework;
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

            var similarityFilename = indexFilename + ".similarity.json";
            // todo check arguments.
            // container.RegisterSingleton<IContentResolver>(new RelativeFilesystemContentResolver(rootPath));
            container.RegisterInstance<IContentResolver>(FilesystemContentResolver.Instance);
            container.RegisterSingleton<IImageDataRepository>(() => new SingleImageDataRepository(new JsonToFileSerializer<List<ImageData>>(indexFilename)));
            container.RegisterSingleton<ISimilarityRepository>(() => new SingleFileSimilarityRepository(new JsonToFileSerializer<List<SimilarityResultStorage>>(similarityFilename)));

            // CQRS lite stuff.
            container.Register<Router, Router>(Lifestyle.Singleton);
            container.Register<ICommandSender, Router>(Lifestyle.Singleton);
            container.Register<IEventPublisher, Router>(Lifestyle.Singleton);
            container.Register<IHandlerRegistrar, Router>(Lifestyle.Singleton);

            container.RegisterSingleton<IEventStore, InMemoryEventStore>();
            container.RegisterSingleton<ICache, MemoryCache>();

            // add scoped?!
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>())); // Repository has two public constructors (why??)
            container.RegisterDecorator<IRepository, CacheRepository>();
            container.Register<ISession, Session>();

            container.Register<IReadModelFacade, ReadModel>();

            // Scan and register command handlers and event handlers
            var coreAssembly = typeof(MediaItemCommandHandlers).Assembly;
            container.Register(typeof(IHandler<>), coreAssembly);
            container.Register(typeof(ICancellableHandler<>), coreAssembly);

            // entity framework stuff??! transient? singleton? ..
            // wip
            container.Register<IMediaItemRepository, EntityFrameworkMediaItemRepository>();

            container.Collection.Register(typeof(IDbContextOptionsStrategy), coreAssembly);
            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            // todo
//            container.Register<IMediaItemDbContextFactory>(() => new MediaItemDbContextFactory(new DbContextOptionsBuilder<MediaItemDbContext>()
//                                                                                               .UseInMemoryDatabase("Dummy")
//                                                                                               .Options));

//            const string connectionString = "InMemory EagleEye";
            const string connectionString = "Filename=./EagleEye.db";
            container.Register<DbContextOptions<MediaItemDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString));
            container.Register<IMediaItemDbContextFactory, MediaItemDbContextFactory>();
//            container.Register<IMediaItemDbContextFactory>(() => new MediaItemDbContextFactory(new DbContextOptionsBuilder<MediaItemDbContext>()
//                                                                                               .UseInMemoryDatabase("Dummy")
//                                                                                               .Options));

            RegisterSearchEngine(container);
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
