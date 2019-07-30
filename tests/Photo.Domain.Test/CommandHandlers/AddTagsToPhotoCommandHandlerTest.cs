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

    public class AddTagsToPhotoCommandHandlerTest
    {
        [NotNull] private readonly AddTagsToPhotoCommandHandler sut;
        [NotNull] private readonly ISession session;
        private readonly Guid photoGuid;
        private readonly CancellationToken ct;

        public AddTagsToPhotoCommandHandlerTest()
        {
            session = A.Fake<ISession>();
            sut = new AddTagsToPhotoCommandHandler(session);
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
            await sut.Handle(new AddTagsToPhotoCommand(photoGuid, 42, "Jake", "Ben"), ct);

            // assert
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenUpdatingPeopleSucceeds()
        {
            // arrange
            var photo = new Photo(photoGuid, "dummy", "dummy2", new byte[32]);
            photo.FlushUncommittedChanges();

            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(photo);

            // act
            await sut.Handle(new AddTagsToPhotoCommand(photoGuid, 42, "Jake", "Ben"), ct);

            // assert
            photo.Persons.Should().BeEquivalentTo("Jake", "Ben");
            photo.GetUncommittedChanges().Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.HaveCount(1)
                .And.AllBeOfType<TagsAddedToPhoto>()
                .And.BeEquivalentTo(new TagsAddedToPhoto(photoGuid, "Jake", "Ben"));
            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => session.Commit(ct)).MustHaveHappenedOnceExactly();
        }
    }
}
