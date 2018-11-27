namespace Photo.ReadModel.Similarity
{
    using System;

    using Helpers.Guards;

    using JetBrains.Annotations;

    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void Bootstrap([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));
            var thisAssembly = typeof(Bootstrapper).Assembly;
            //
            // container.Register<IEagleEyeRepository, EntityFrameworkSimilarityRepository>();
            // container.Collection.Register(typeof(IDbContextOptionsStrategy), thisAssembly);
            //
            // container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            // container.Register<DbContextOptions<EagleEyeDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString));
            // container.Register<ISimilarityDbContextFactory>(
            //     () =>
            //     {
            //         // arghhh... todo
            //         var result = container.GetInstance<SimilarityDbContextFactory>();
            //         result.Initialize().GetAwaiter().GetResult();
            //         return result;
            //     },
            //     Lifestyle.Singleton);

            // container.RegisterSingleton<PhotoIndex>();
            // container.Register<ISimilarityReadModel, ReadModelEntityFramework>();

            // container.Register<SimilarityEventHandlers>();
        }

        public static Type[] GetEventHandlerTypes()
        {
            return new Type[]
            {
                // typeof(SimilarityEventHandlers),
            };
        }
    }
}
