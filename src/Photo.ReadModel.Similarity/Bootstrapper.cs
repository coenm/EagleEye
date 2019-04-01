namespace EagleEye.Photo.ReadModel.Similarity
{
    using System;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using EagleEye.Photo.ReadModel.Similarity.Internal.SimpleInjectorAdapter;
    using Hangfire;
    using Hangfire.MemoryStorage;
    using Hangfire.SQLite;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
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
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace();
            Guard.Argument(hangFireConnectionString, nameof(hangFireConnectionString)).NotNull().NotWhiteSpace();
            var thisAssembly = typeof(Bootstrapper).Assembly;

            container.Register<IInternalStatelessSimilarityRepository, InternalSimilarityRepository>(Lifestyle.Singleton);
            container.Collection.Register(typeof(IDbContextOptionsStrategy), thisAssembly);

            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            container.Register<DbContextOptions<SimilarityDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString), Lifestyle.Singleton);
            container.Register<ISimilarityDbContextFactory, SimilarityDbContextFactory>(Lifestyle.Singleton);

            container.Register<SimilarityEventHandlers>();

            container.Collection.Register<IEagleEyeInitialize>(typeof(DatabaseInitializer));

            // BackgroundJobClient contains multiple public constructors.
            container.Register<IBackgroundJobClient>(() => new BackgroundJobClient(), Lifestyle.Singleton);

            container.Register<ISimilarityJobConfiguration>(() => new StaticSimilarityJobConfiguration(80d), Lifestyle.Singleton);

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
