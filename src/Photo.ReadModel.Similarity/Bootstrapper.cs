namespace Photo.ReadModel.Similarity
{
    using System;

    using Helpers.Guards;

    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Interface;
    using Photo.ReadModel.Similarity.Internal;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EventHandlers;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <param name="connectionString">Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void Bootstrap([NotNull] Container container, [NotNull] string connectionString)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            var thisAssembly = typeof(Bootstrapper).Assembly;

            container.Register<ISimilarityRepository, EntityFrameworkSimilarityRepository>();
            container.Collection.Register(typeof(IDbContextOptionsStrategy), thisAssembly);

            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            container.Register<DbContextOptions<SimilarityDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString));
            container.Register<ISimilarityDbContextFactory>(
                () =>
                {
                    // arghhh... todo
                    var result = container.GetInstance<SimilarityDbContextFactory>();
                    result.Initialize().GetAwaiter().GetResult();
                    return result;
                },
                Lifestyle.Singleton);

            container.Register<ISimilarityReadModel, ReadModelEntityFramework>();
            container.Register<SimilarityEventHandlers>();
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
