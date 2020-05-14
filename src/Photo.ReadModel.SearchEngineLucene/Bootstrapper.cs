namespace EagleEye.Photo.ReadModel.SearchEngineLucene
{
    using System;

    using Dawn;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Bootstrapper
    {
        /// <summary> Bootstrap this module.</summary>
        /// <param name="container">The IOC container. Cannot be <c>null</c>.</param>
        /// <param name="baseDirectory">Base directory for the Lucene index files. <c>null</c> Or an empty string will result in an InMemory index.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the required arguments is <c>null</c>.</exception>
        public static void BootstrapSearchEngineLuceneReadModel(
            [NotNull] Container container,
            [CanBeNull] string baseDirectory = null)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.RegisterSingleton<IPhotoIndex, PhotoIndex>();

            container.Register<IReadModel, LucenePhotoReadModel>();

            container.Register<PhotoCreatedEventHandler>();
            container.Register<PersonsAddedToPhotoEventHandler>();
            container.Register<PersonsRemovedFromPhotoEventHandler>();
            container.Register<TagsAddedToPhotoEventHandler>();
            container.Register<TagsRemovedFromPhotoEventHandler>();
            container.Register<LocationSetToPhotoEventHandler>();
            container.Register<LocationClearedFromPhotoEventHandler>();
            container.Register<DateTimeTakenChangedEventHandler>();

            if (string.IsNullOrWhiteSpace(baseDirectory))
                container.RegisterSingleton<ILuceneDirectoryFactory, RamLuceneDirectoryFactory>();
            else
                container.RegisterSingleton<ILuceneDirectoryFactory>(() => new FileSystemLuceneDirectoryFactory(baseDirectory));
        }

        public static Type[] GetEventHandlerTypes()
        {
            return new[]
            {
                typeof(PhotoCreatedEventHandler),
                typeof(PersonsAddedToPhotoEventHandler),
                typeof(PersonsRemovedFromPhotoEventHandler),
                typeof(TagsAddedToPhotoEventHandler),
                typeof(TagsRemovedFromPhotoEventHandler),
                typeof(LocationSetToPhotoEventHandler),
                typeof(LocationClearedFromPhotoEventHandler),
                typeof(DateTimeTakenChangedEventHandler),
            };
        }
    }
}
