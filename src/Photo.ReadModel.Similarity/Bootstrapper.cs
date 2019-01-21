namespace Photo.ReadModel.Similarity
{
    using System;

    using EagleEye.Core.Interfaces;
    using Hangfire;
    using Hangfire.MemoryStorage;
    using Hangfire.SQLite;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Interface;
    using Photo.ReadModel.Similarity.Internal;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EventHandlers;
    using Photo.ReadModel.Similarity.Internal.Processing;
    using Photo.ReadModel.Similarity.Internal.SimpleInjectorAdapter;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <param name="connectionString">Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty.</param>
        /// <param name="hangFireConnectionString">Connection string for HangFire.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void Bootstrap(
            [NotNull] Container container,
            [NotNull] string connectionString,
            [NotNull] string hangFireConnectionString)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            Guard.NotNullOrWhiteSpace(hangFireConnectionString, nameof(hangFireConnectionString));
            var thisAssembly = typeof(Bootstrapper).Assembly;

            container.Register<IInternalStatelessSimilarityRepository, InternalSimilarityRepository>(Lifestyle.Singleton);
            container.Collection.Register(typeof(IDbContextOptionsStrategy), thisAssembly);

            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            container.Register<DbContextOptions<SimilarityDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString), Lifestyle.Singleton);
            container.Register<ISimilarityDbContextFactory, SimilarityDbContextFactory>(Lifestyle.Singleton);

            container.Register<ISimilarityReadModel, ReadModelEntityFramework>();
            container.Register<SimilarityEventHandlers>();

            container.Collection.Register<IEagleEyeInitialize>(typeof(DatabaseInitializer));

            // BackgroundJobClient contains multiple public constructors.
            container.Register<IBackgroundJobClient>(() => new BackgroundJobClient(), Lifestyle.Singleton);

            // todo
            container.RegisterSingleton<HangFireServerEagleEyeProcess, HangFireServerEagleEyeProcess>();
            container.Collection.Append<IEagleEyeProcess, HangFireServerEagleEyeProcess>();
            container.Collection.Append<IEagleEyeInitialize, ModuleInitializer>();

            SetHangFireConfiguration(container, hangFireConnectionString);
        }

        public static Type[] GetEventHandlerTypes()
        {
            return new Type[]
            {
                typeof(SimilarityEventHandlers),
            };
        }

        private static void SetHangFireConfiguration(Container container, string connectionString)
        {
            // TODO this is not very nice  :|
            if (connectionString.StartsWith("InMemory"))
            {
                Hangfire.GlobalConfiguration
                    .Configuration
                    .UseActivator(new SimpleInjectorJobActivator(container))
                    .UseMemoryStorage();
            }
            else
            {
                Hangfire.GlobalConfiguration
                    .Configuration
                    .UseSQLiteStorage(connectionString)
                    .UseActivator(new SimpleInjectorJobActivator(container));
            }
        }
    }
}
