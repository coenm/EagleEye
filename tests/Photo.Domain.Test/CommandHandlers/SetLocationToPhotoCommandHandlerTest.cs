namespace EagleEye.Photo.Domain.Test.CommandHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Domain;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.Domain.Events;
    using FakeItEasy;
    using FluentAssertions;
    using JetBrains.Annotations;
    using Xunit;

    public class SetLocationToPhotoCommandHandlerTest
    {
        [NotNull] private readonly SetLocationToPhotoCommandHandler sut;
        [NotNull] private readonly ISession session;
        private readonly Guid photoGuid;
        private readonly CancellationToken ct;

        public SetLocationToPhotoCommandHandlerTest()
        {
            session = A.Fake<ISession>();
            sut = new SetLocationToPhotoCommandHandler(session);
            photoGuid = Guid.NewGuid();
            ct = default;
        }

        [Fact]
        public async Task Handle_ShouldGetAggregateFromSession()
        {
            // arrange
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(new Photo(photoGuid, "dummy", "dummy2", new byte[32]));

            // act
            await sut.Handle(new SetLocationToPhotoCommand(photoGuid, 42), ct);

            // assert
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenUpdatingPhotoHashSucceeds()
        {
            // arrange
            var photo = new Photo(photoGuid, "dummy", "dummy2", new byte[32]);
            photo.FlushUncommittedChanges();

            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(photo);

            // act
            await sut.Handle(
                new SetLocationToPhotoCommand(photoGuid, 42)
                    {
                        City = "new city",
                        CountryCode = "new countrycode",
                        CountryName = "new country name",
                        State = "new state",
                        SubLocation = "new sub location",
                        Latitude = 133,
                        Longitude = 123,
                    },
                ct);

            // assert
            var expectedLocation = new Location(
                "new countrycode",
                "new country name",
                "new state",
                "new city",
                "new sub location",
                123,
                133);

            photo.GetUncommittedChanges().Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.HaveCount(1)
                .And.AllBeOfType<LocationSetToPhoto>()
                .And.BeEquivalentTo(new LocationSetToPhoto(photoGuid, expectedLocation));
            photo.Location.Should().BeEquivalentTo(expectedLocation);
            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => session.Commit(ct)).MustHaveHappenedOnceExactly();
        }
    }
}
