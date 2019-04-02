namespace EagleEye.Photo.Domain
{
    using System;

    using CQRSlite.Commands;
    using CQRSlite.Messages;
    using CQRSlite.Queries;
    using Dawn;
    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.Decorators;
    using EagleEye.Photo.Domain.Services;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void BootstrapPhotoDomain([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            var thisAssembly = typeof(Bootstrapper).Assembly;

            container.Register(typeof(IHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICommandHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableCommandHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(IQueryHandler<,>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableQueryHandler<,>), thisAssembly, Lifestyle.Transient);

            container.RegisterDecorator(typeof(ICancellableCommandHandler<>), typeof(VerifyTokenCommandHandlerDecorator<>), Lifestyle.Transient);

            container.Register<IUniqueFilenameService, UniqueFilenameService>(Lifestyle.Singleton);
            container.Register<IFilenameRepository, InMemoryFilenameRepository>(Lifestyle.Singleton);

            container.Register<MediaItemCommandHandlers>();
            container.Register<CreatePhotoCommandHandler>();
        }

        public static Type[] GetEventHandlerTypesPhotoDomain()
        {
            return new Type[]
            {
                typeof(CreatePhotoCommandHandler),
                typeof(MediaItemCommandHandlers),
            };
        }
    }
}
