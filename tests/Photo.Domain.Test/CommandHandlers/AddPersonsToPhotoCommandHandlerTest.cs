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

    public class AddPersonsToPhotoCommandHandlerTest
    {
        [NotNull] private AddPersonsToPhotoCommandHandler sut;
        [NotNull] private ISession session;
        private Guid photoGuid;
        private CancellationToken ct;

        public AddPersonsToPhotoCommandHandlerTest()
        {
            session = A.Fake<ISession>();
            sut = new AddPersonsToPhotoCommandHandler(session);
            photoGuid = Guid.NewGuid();
            ct = default;
        }

        [Fact]
        public async Task Handle_ShouldGetAggregateFromSession()
        {
            // arrange
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(new Photo(photoGuid, "d", "x", new byte[32]));

            // act
            await sut.Handle(new AddPersonsToPhotoCommand(photoGuid, 42, "Jake", "Ben"), ct);

            // assert
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenUpdatingPeopleSucceeds()
        {
            // arrange
            var photo = new Photo(photoGuid, "d", "x", new byte[32]);
            photo.FlushUncommittedChanges();

            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(photo);

            // act
            await sut.Handle(new AddPersonsToPhotoCommand(photoGuid, 42, "Jake", "Ben"), ct);

            // assert
            photo.Persons.Should().BeEquivalentTo("Jake", "Ben");
            photo.GetUncommittedChanges().Should().BeEquivalentTo(new PersonsAddedToPhoto(photoGuid, "Jake", "Ben"));
            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => session.Commit(ct)).MustHaveHappenedOnceExactly();
        }
    }
}
