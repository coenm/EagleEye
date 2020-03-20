namespace Photo.ReadModel.SearchEngineLucene.Test.Internal.EventHandlers
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    using TimestampPrecision = EagleEye.Photo.Domain.Aggregates.TimestampPrecision;

    public class DateTimeTakenChangedEventHandlerTest
    {
        private readonly DateTimeTakenChangedEventHandler sut;
        private readonly IPhotoIndex photoIndex;
        private readonly EagleEye.Photo.Domain.Aggregates.Timestamp eventDateTime;

        public DateTimeTakenChangedEventHandlerTest()
        {
            photoIndex = A.Fake<IPhotoIndex>();
            sut = new DateTimeTakenChangedEventHandler(photoIndex);
            eventDateTime = EagleEye.Photo.Domain.Aggregates.Timestamp.Create(2021, 7, 25, 23, 55, 32);
        }

        [Fact]
        public async Task Handle_ShouldSearchForPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(new DateTimeTakenChanged(guid, eventDateTime));

            // assert
            A.CallTo(() => photoIndex.Search(guid)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenPhotoDoesNotExist()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => photoIndex.Search(guid)).Returns(null);

            // act
            await sut.Handle(new DateTimeTakenChanged(guid, eventDateTime));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithUpdatedDateTime_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photoSearchResult = new PhotoSearchResult(1)
            {
                DateTimeTaken = Timestamp.FromDateTime(new DateTime(1999, 1, 1)),
            };

            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => photoIndex.Search(guid)).Returns(photoSearchResult);

            // act
            await sut.Handle(new DateTimeTakenChanged(guid, eventDateTime));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            var expectedTimestamp = new Timestamp(eventDateTime.Value, EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model.TimestampPrecision.Second);
            newPhoto.DateTimeTaken.Should().BeEquivalentTo(expectedTimestamp);
        }
    }
}
