namespace EagleEye.Photo.Domain
{
    using System;

    using CQRSlite.Commands;
    using CQRSlite.Messages;
    using CQRSlite.Queries;
    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.Decorators;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void BootstrapPhotoDomain([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));
            var thisAssembly = typeof(Bootstrapper).Assembly;

            container.Register(typeof(IHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICommandHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableCommandHandler<>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(IQueryHandler<,>), thisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableQueryHandler<,>), thisAssembly, Lifestyle.Transient);

            container.RegisterDecorator(typeof(ICancellableCommandHandler<>), typeof(VerifyTokenCommandHandlerDecorator<>), Lifestyle.Transient);

            container.Register<MediaItemCommandHandlers>();
        }

        public static Type[] GetEventHandlerTypesPhotoDomain()
        {
            return new Type[]
            {
                typeof(MediaItemCommandHandlers),
            };
        }
    }
}
