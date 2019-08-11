namespace EagleEye.Photo.ReadModel.EntityFramework
{
    using System;
    using System.Reflection;

    using CQRSlite.Commands;
    using CQRSlite.Messages;
    using CQRSlite.Queries;
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
        /// <param name="connectionString">Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty.</param>
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

            RegisterEventHandler(container);

            container.Collection.Append<IEagleEyeInitialize, ModuleInitializer>();
        }

        private static void RegisterEventHandler(Container container)
        {
            container.Register(typeof(IHandler<>), ThisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableHandler<>), ThisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICommandHandler<>), ThisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableCommandHandler<>), ThisAssembly, Lifestyle.Transient);
            container.Register(typeof(IQueryHandler<,>), ThisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableQueryHandler<,>), ThisAssembly, Lifestyle.Transient);
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
