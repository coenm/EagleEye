namespace Photo.ReadModel.Similarity
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;

    using Hangfire;
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
            container.Register<ISimilarityDbContextFactory>(
                () =>
                {
                    // arghhh... todo
                    var result = container.GetInstance<SimilarityDbContextFactory>();
                 //   result.Initialize().GetAwaiter().GetResult();
                    return result;
                },
                Lifestyle.Singleton);

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

        public static Task InitAsync([NotNull] Container container, CancellationToken ct = default(CancellationToken))
        {
            Guard.NotNull(container, nameof(container));

            return Task.CompletedTask;
        }

        // public static void Run([NotNull] Container container, [NotNull] string hangfireDatabaseConnectionString, CancellationToken ct = default(CancellationToken))
        // {
        //     Guard.NotNull(container, nameof(container));
        //     Guard.NotNullOrWhiteSpace(hangfireDatabaseConnectionString, nameof(hangfireDatabaseConnectionString));
        //
        //     if (ct.IsCancellationRequested)
        //         return;
        //
        //     var mre = new ManualResetEvent(false);
        //
        //     Hangfire.GlobalConfiguration
        //             .Configuration
        //             .UseSQLiteStorage(hangfireDatabaseConnectionString)
        //             .UseActivator(new SimpleInjectorJobActivator(container));
        //
        //     var options = new BackgroundJobServerOptions
        //                   {
        //                       WorkerCount = 1,
        //                   };
        //     var server = new BackgroundJobServer(options);
        //
        //     Task.Run(
        //              () =>
        //              {
        //                  using (ct.Register(() => mre.Set()))
        //                  {
        //                      mre.WaitOne();
        //                  }
        //
        //                  server.SendStop();
        //                  Thread.Sleep(100);
        //                  server.Dispose();
        //              },
        //              ct);
        // }

        private static void RunCalculationAsync(
            [NotNull] Container container,
            [NotNull] string hangfireConnectionString,
            CancellationToken ct)
        {
            DebugGuard.NotNull(container, nameof(container));
            DebugGuard.NotNullOrWhiteSpace(hangfireConnectionString, nameof(hangfireConnectionString));

            if (ct.IsCancellationRequested)
                return;

            var mre = new ManualResetEvent(false);

            Hangfire.GlobalConfiguration
                    .Configuration
                    .UseSQLiteStorage(hangfireConnectionString)
                    .UseActivator(new SimpleInjectorJobActivator(container));

            using (var server = new BackgroundJobServer())
            {
                using (ct.Register(() => mre.Set()))
                {
                    mre.WaitOne();
                }
            }
        }
    }
}
