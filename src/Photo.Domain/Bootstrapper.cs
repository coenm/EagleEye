namespace EagleEye.Photo.Domain
{
    using System;
    using System.Reflection;

    using CQRSlite.Commands;
    using Dawn;
    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.Decorators;
    using EagleEye.Photo.Domain.Services;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Bootstrapper
    {
        private static readonly Assembly ThisAssembly = typeof(Bootstrapper).Assembly;

        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void BootstrapPhotoDomain([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.Register(typeof(ICommandHandler<>), ThisAssembly, Lifestyle.Transient);
            container.Register(typeof(ICancellableCommandHandler<>), ThisAssembly, Lifestyle.Transient);

            container.RegisterDecorator(typeof(ICancellableCommandHandler<>), typeof(VerifyTokenCommandHandlerDecorator<>), Lifestyle.Transient);

            container.Register<IUniqueFilenameService, UniqueFilenameService>(Lifestyle.Singleton);
            container.Register<IFilenameRepository, InMemoryFilenameRepository>(Lifestyle.Singleton);
        }

        public static Type[] GetEventHandlerTypesPhotoDomain()
        {
            return new Type[]
            {
                typeof(CreatePhotoCommandHandler),
                typeof(UpdateFileHashCommandHandler),
                typeof(AddPersonsToPhotoCommandHandler),
                typeof(RemovePersonsFromPhotoCommandHandler),
                typeof(AddTagsToPhotoCommandHandler),
                typeof(RemoveTagsFromPhotoCommandHandler),
                typeof(ClearPhotoHashCommandHandler),
                typeof(UpdatePhotoHashCommandHandler),
                typeof(SetLocationToPhotoCommandHandler),
                typeof(ClearLocationFromPhotoCommandHandler),
                typeof(SetDateTimeTakenCommandHandler),
            };
        }
    }
}
