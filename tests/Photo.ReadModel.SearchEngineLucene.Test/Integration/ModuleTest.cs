namespace Photo.ReadModel.SearchEngineLucene.Test.Integration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class ModuleTest
    {
        private readonly ICancellableEventHandler<PhotoCreated> handlerPhotoCreated;
        private readonly ICancellableEventHandler<PersonsAddedToPhoto> handlerPersonsAddedToPhoto;
        private readonly IReadModel readModel;

        public ModuleTest()
        {
            var container = new Container();
            EagleEye.Photo.ReadModel.SearchEngineLucene.Bootstrapper.BootstrapSearchEngineLuceneReadModel(container, true, null);
            container.Register(typeof(ICancellableEventHandler<>), EagleEye.Photo.ReadModel.SearchEngineLucene.Bootstrapper.GetEventHandlerTypes());

            handlerPhotoCreated = container.GetInstance<ICancellableEventHandler<PhotoCreated>>();
            handlerPersonsAddedToPhoto = container.GetInstance<ICancellableEventHandler<PersonsAddedToPhoto>>();

            readModel = container.GetInstance<IReadModel>();
        }

        /// <summary>
        /// Add three photos with persons. These should be indexed and searchable.
        /// </summary>
        [Fact]
        public async Task IndexAndSearchForPersonsInPhotos()
        {
            // arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            // act
            await handlerPhotoCreated.Handle(new PhotoCreated(guid1, "a/b/c.jpg", "image/jpeg", new byte[8]));
            await handlerPersonsAddedToPhoto.Handle(new PersonsAddedToPhoto(guid1, "Adam", "Bob", "Calvin"));

            await handlerPhotoCreated.Handle(new PhotoCreated(guid2, "a/b/dexter.jpg", "image/jpeg", new byte[8]));
            await handlerPersonsAddedToPhoto.Handle(new PersonsAddedToPhoto(guid2, "Bob", "Dexter"));

            await handlerPhotoCreated.Handle(new PhotoCreated(guid3, "a/bobby.jpg", "image/jpeg", new byte[8]));
            await handlerPersonsAddedToPhoto.Handle(new PersonsAddedToPhoto(guid3, "Adam", "bob"));

            const string query = "person:Bob -person:ca*";
            var result1 = readModel.FullSearch(query);
            var result2 = readModel.Search(query);
            var result3 = readModel.Count(query);

            // assert
            result1.Should().HaveCount(2);
            result1.Select(x => x.Id).Should().BeEquivalentTo(guid2, guid3);

            result2.Should().HaveCount(2);
            result2.Select(x => x.Id).Should().BeEquivalentTo(guid2, guid3);

            result3.Should().Be(2);
        }
    }
}
