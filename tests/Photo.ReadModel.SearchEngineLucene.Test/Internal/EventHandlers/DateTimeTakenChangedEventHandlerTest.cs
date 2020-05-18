namespace Photo.ReadModel.SearchEngineLucene.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    using Domain = EagleEye.Photo.Domain.Aggregates;
    using ReadModel = EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;

    public class DateTimeTakenChangedEventHandlerTest
    {
        private readonly DateTimeTakenChangedEventHandler sut;
        private readonly IPhotoIndex photoIndex;
        private readonly Domain.Timestamp eventDateTime;

        public DateTimeTakenChangedEventHandlerTest()
        {
            photoIndex = A.Fake<IPhotoIndex>();
            sut = new DateTimeTakenChangedEventHandler(photoIndex);
            eventDateTime = Domain.Timestamp.Create(2021, 7, 25, 23, 55, 32);
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
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<ReadModel.Photo>._)).MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(Timestamps))]

        internal async Task Handle_ShouldReIndexPhotoWithUpdatedDateTime_WhenPhotoExists(Domain.Timestamp eventTimestamp, ReadModel.Timestamp expectedTimestamp)
        {
            // arrange
            var guid = Guid.NewGuid();
            ReadModel.Photo newPhoto = null;
            var photoSearchResult = new ReadModel.PhotoSearchResult(1)
                {
                    DateTimeTaken = ReadModel.Timestamp.FromDateTime(new DateTime(1999, 1, 1)),
                };

            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<ReadModel.Photo>._)).Invokes(call => newPhoto = call.Arguments[0] as ReadModel.Photo);
            A.CallTo(() => photoIndex.Search(guid)).Returns(photoSearchResult);

            // act
            await sut.Handle(new DateTimeTakenChanged(guid, eventTimestamp));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<ReadModel.Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.DateTimeTaken.Should().BeEquivalentTo(expectedTimestamp);
        }

        public static IEnumerable<object[]> Timestamps()
        {
            yield return new object[] { Domain.Timestamp.Create(2020), new ReadModel.Timestamp(new DateTime(2020, 1, 1), ReadModel.TimestampPrecision.Year), };
            yield return new object[] { Domain.Timestamp.Create(2020, 6), new ReadModel.Timestamp(new DateTime(2020, 6, 1), ReadModel.TimestampPrecision.Month), };
            yield return new object[] { Domain.Timestamp.Create(2020, 9, 4), new ReadModel.Timestamp(new DateTime(2020, 9, 4), ReadModel.TimestampPrecision.Day), };
            yield return new object[] { Domain.Timestamp.Create(2020, 12, 4, 14), new ReadModel.Timestamp(new DateTime(2020, 12, 4, 14, 0, 0), ReadModel.TimestampPrecision.Hour), };
            yield return new object[] { Domain.Timestamp.Create(2020, 12, 4, 12, 51), new ReadModel.Timestamp(new DateTime(2020, 12, 4, 12, 51, 0), ReadModel.TimestampPrecision.Minute), };
            yield return new object[] { Domain.Timestamp.Create(2020, 12, 4, 14, 1, 0), new ReadModel.Timestamp(new DateTime(2020, 12, 4, 14, 1, 0), ReadModel.TimestampPrecision.Second), };
        }
    }
}
