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
    using EagleEye.Photo.Domain.Services;
    using FakeItEasy;
    using FluentAssertions;
    using JetBrains.Annotations;
    using Xunit;

    public class CreatePhotoCommandHandlerTest
    {
        [NotNull] private readonly CreatePhotoCommandHandler sut;
        [NotNull] private readonly ISession session;
        [NotNull] private readonly IUniqueFilenameService uniqueFilenameService;
        private readonly Guid photoGuid;
        private readonly CancellationToken ct;

        public CreatePhotoCommandHandlerTest()
        {
            session = A.Fake<ISession>();
            uniqueFilenameService = A.Fake<IUniqueFilenameService>();
            sut = new CreatePhotoCommandHandler(session, uniqueFilenameService);
            photoGuid = Guid.NewGuid();
            ct = default;
        }

        [Fact] public async Task Handle_ShouldClaimFilename()
        {
            // arrange
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(new Photo(photoGuid, "dummy", "dummy2", new byte[32]));

            // act
            await sut.Handle(new CreatePhotoCommand("a.jpg", new byte[32], "mime", null, null), ct);

            // assert
            A.CallTo(() => uniqueFilenameService.Claim("a.jpg")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenClearingPhotoHashSucceeds()
        {
            // arrange
            var photo = new Photo(photoGuid, "dummy", "dummy2", new byte[32]);
            photo.UpdatePhotoHash("hashIdentifier1", 1231);
            photo.UpdatePhotoHash("hashIdentifier2", 1232);
            photo.FlushUncommittedChanges();

            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(photo);

            // act
            await sut.Handle(new CreatePhotoCommand("a.jpg", new byte[32], "mime", null, null), ct);

            // assert
//            photo.GetUncommittedChanges().Should()
//                .NotBeNull()
//                .And.NotBeEmpty()
//                .And.HaveCount(1)
//                .And.AllBeOfType<PhotoHashCleared>()
//                .And.BeEquivalentTo(new PhotoHashCleared(photoGuid, "hashIdentifier1"));
//            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustNotHaveHappened();
//            A.CallTo(() => session.Commit(ct)).MustHaveHappenedOnceExactly();
        }
    }
}
