namespace EagleEye.Photo.ReadModel.EntityFramework
{
    using System;
    using System.Reflection;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.ReadModel.EntityFramework.Interface;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.ContextOptions;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EventHandlers;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using SimpleInjector;

    public static class Bootstrapper
    {
        private static readonly Assembly ThisAssembly = typeof(Bootstrapper).Assembly;

        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <param name="connectionString">Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty. Should start with 'InMemory' or with 'Filename='.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void BootstrapEntityFrameworkReadModel([NotNull] Container container, [NotNull] string connectionString)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace();

            container.Register<IEagleEyeRepository, EntityFrameworkEagleEyeRepository>();
            RegisterDbContextOptions(container);

            container.Register<DbContextOptionsFactory>(Lifestyle.Singleton);
            container.Register<DbContextOptions<EagleEyeDbContext>>(() => container.GetInstance<DbContextOptionsFactory>().Create(connectionString), Lifestyle.Singleton);
            container.Register<IEagleEyeDbContextFactory, EagleEyeDbContextFactory>(Lifestyle.Singleton);

            container.Register<IReadModelEntityFramework, ReadModelEntityFramework>();

            /*
             todo change registration of event handlers.
            container.Register(typeof(ICancellableEventHandler<>), GetEventHandlerTypes(), Lifestyle.Transient);
            */

            container.Collection.Append<IEagleEyeInitialize, ModuleInitializer>();
        }

        public static Type[] GetEventHandlerTypes()
        {
            return new Type[]
            {
                typeof(DateTimeTakenChangedEventHandler),
                typeof(LocationClearedFromPhotoEventHandler),
                typeof(LocationSetToPhotoEventHandler),
                typeof(PersonsAddedToPhotoEventHandler),
                typeof(PersonsRemovedFromPhotoEventHandler),
                typeof(PhotoCreatedEventHandler),
                typeof(TagsAddedToPhotoEventHandler),
                typeof(TagsRemovedFromPhotoEventHandler),
            };
        }

        private static void RegisterDbContextOptions(Container container)
        {
            // original:
            // container.Collection.Register(typeof(IDbContextOptionsStrategy), thisAssembly);
            // workaround https://stackoverflow.com/questions/52777116/simple-injector-how-to-register-resolve-collection-of-singletons-against-same-i
            container.Collection.Register<IDbContextOptionsStrategy>(
                                                                     new[]
                                                                     {
                                                                         Lifestyle.Singleton.CreateRegistration<IDbContextOptionsStrategy>(container.GetInstance<SqlLiteDatabaseOptionsBuilder>, container),
                                                                         Lifestyle.Singleton.CreateRegistration<IDbContextOptionsStrategy>(container.GetInstance<InMemoryDatabaseOptionsBuilder>, container),
                                                                     });
        }
    }
}
