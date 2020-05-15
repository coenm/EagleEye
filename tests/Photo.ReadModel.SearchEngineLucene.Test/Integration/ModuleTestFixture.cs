namespace Photo.ReadModel.SearchEngineLucene.Test.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using JetBrains.Annotations;
    using SimpleInjector;

    /// <summary>
    /// Fires a number of events that should update the Lucene search index. Eventually, tree files should have been indexed with a number of persons.
    /// </summary>
    [UsedImplicitly]
    public class ModuleTestFixture : IDisposable
    {
        private readonly Task initializeTask;
        private readonly Container container;
        private readonly ICancellableEventHandler<PhotoCreated> handlerPhotoCreated;
        private readonly ICancellableEventHandler<PersonsAddedToPhoto> handlerPersonsAddedToPhoto;

        public ModuleTestFixture()
        {
            container = new Container();
            EagleEye.Photo.ReadModel.SearchEngineLucene.Bootstrapper.BootstrapSearchEngineLuceneReadModel(container);

            container.Register(typeof(ICancellableEventHandler<>), EagleEye.Photo.ReadModel.SearchEngineLucene.Bootstrapper.GetEventHandlerTypes());

            handlerPhotoCreated = container.GetInstance<ICancellableEventHandler<PhotoCreated>>();
            handlerPersonsAddedToPhoto = container.GetInstance<ICancellableEventHandler<PersonsAddedToPhoto>>();

            ReadModel = container.GetInstance<IReadModel>();

            initializeTask = InitializeImpl();
        }

        public IReadModel ReadModel { get; }

        /// <summary>
        /// a/b/x/c.jpg [Adam, Bob, Calvin]
        /// </summary>
        public static PhotoPersonItem Photo1 { get; } = new PhotoPersonItem(
            new Guid("F0C78074-8AB8-416B-8A2B-DC066250F9F1"),
            "a/b/x/c.jpg",
            "Adam", "Bob", "Calvin");

        /// <summary>
        /// a/e/dexter.jpg [Bob Jackson, Dexter]
        /// </summary>
        public static PhotoPersonItem Photo2 { get; } = new PhotoPersonItem(
            new Guid("F0C78074-8AB8-416B-8A2B-DC066250F9F2"),
            "a/b/dexter.jpg",
            "Bob Jackson", "Dexter");

        /// <summary>
        /// a/bobby.jpg [Adam, bob]
        /// </summary>
        public static PhotoPersonItem Photo3 { get; } = new PhotoPersonItem(
            new Guid("F0C78074-8AB8-416B-8A2B-DC066250F9F3"),
            "a/bobby.jpg",
            "Adam", "bob");

        /// <summary>
        /// b/x/red bulls/TeamPhoto.jpg [Michael Jordan, Scottie Pippen]
        /// </summary>
        public static PhotoPersonItem Photo4 { get; } = new PhotoPersonItem(
            new Guid("F0C78074-8AB8-416B-8A2B-DC066250F9F4"),
            "b/x/red bulls/TeamPhoto.jpg",
            "Michael Jordan", "Scottie Pippen");

        public Task Initialize() => initializeTask;

        public void Dispose() => container.Dispose();

        private async Task InitializeImpl()
        {
            foreach (var photo in new[] { Photo1, Photo2, Photo3, Photo4, })
            {
                await handlerPhotoCreated.Handle(new PhotoCreated(photo.Guid, photo.Filename, "image/jpeg", new byte[8])).ConfigureAwait(false);
                await handlerPersonsAddedToPhoto.Handle(new PersonsAddedToPhoto(photo.Guid, photo.Persons.ToArray())).ConfigureAwait(false);
            }
        }

        public class PhotoPersonItem
        {
            public PhotoPersonItem(Guid guid, string filename, params string[] persons)
            {
                Guid = guid;
                Filename = filename;
                Persons = persons;
            }

            public Guid Guid { get; }

            public string Filename { get; }

            public IReadOnlyList<string> Persons { get; }
        }
    }
}
