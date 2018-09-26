﻿namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;

    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;

    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.JsonSimilarity;
    using EagleEye.FileImporter.Infrastructure.PersistantSerializer;
    using EagleEye.FileImporter.Similarity;

    using SimpleInjector;

    using CQRSlite.Events;
    using CQRSlite.Routing;

    using EagleEye.Core.Domain;

    using JetBrains.Annotations;

    public static class Startup
    {
        public static void ConfigureContainer([NotNull] Container container, [NotNull] string indexFilename)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (indexFilename == null)
                throw new ArgumentNullException(nameof(indexFilename));

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
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}