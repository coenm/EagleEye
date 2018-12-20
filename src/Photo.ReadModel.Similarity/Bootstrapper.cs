namespace Photo.ReadModel.Similarity
{
    using System;

    using EagleEye.Core.Interfaces;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Interface;
    using Photo.ReadModel.Similarity.Internal;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EventHandlers;
    using Photo.ReadModel.Similarity.Internal.Processing;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <param name="connectionString">Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty.</param>
        /// <param name="hangfireConnectionString">Connection string for hangfire.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void Bootstrap(
            [NotNull] Container container,
            [NotNull] string connectionString,
            [NotNull] string hangfireConnectionString)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            Guard.NotNullOrWhiteSpace(hangfireConnectionString, nameof(hangfireConnectionString));
            var thisAssembly = typeof(Bootstrapper).Assembly;

            container.Register<ISimilarityRepository, EntityFrameworkSimilarityRepository>();
            container.Collection.Register(typeof(IDbContextOptionsStrategy), thisAssembly);

            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            container.Register<DbContextOptions<SimilarityDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString));
            container.Register<ISimilarityDbContextFactory, SimilarityDbContextFactory>(Lifestyle.Singleton);

            container.Register<ISimilarityReadModel, ReadModelEntityFramework>();
            container.Register<SimilarityEventHandlers>();

            container.Collection.Register<IEagleEyeInitialize>(typeof(DatabaseInitializer));

            // todo
            container.RegisterSingleton<HangFireServerEagleEyeProcess>(() => new HangFireServerEagleEyeProcess(container, hangfireConnectionString));
            container.Collection.Append<IEagleEyeProcess, HangFireServerEagleEyeProcess>();
            container.Collection.Append<IEagleEyeInitialize, ModuleInitializer>();
        }

        public static Type[] GetEventHandlerTypes()
        {
            return new Type[]
            {
                typeof(SimilarityEventHandlers),
            };
        }
    }
}
