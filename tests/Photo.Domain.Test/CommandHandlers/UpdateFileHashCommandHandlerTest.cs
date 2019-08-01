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

    public class UpdateFileHashCommandHandlerTest
    {
        [NotNull] private readonly UpdateFileHashCommandHandler sut;
        [NotNull] private readonly ISession session;
        private readonly Guid photoGuid;
        private readonly CancellationToken ct;

        public UpdateFileHashCommandHandlerTest()
        {
            session = A.Fake<ISession>();
            sut = new UpdateFileHashCommandHandler(session);
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
            await sut.Handle(new UpdateFileHashCommand(photoGuid, 42, new byte[32]), ct);

            // assert
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenUpdatingFileHashSucceeds()
        {
            // arrange
            var hash = CreatePseudoRandomByteArray(32, 42);
            var photo = new Photo(photoGuid, "dummy", "dummy2", CreatePseudoRandomByteArray(32, 16));
            photo.FlushUncommittedChanges();

            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(photo);

            // act
            await sut.Handle(new UpdateFileHashCommand(photoGuid, 42, hash), ct);

            // assert
            photo.GetUncommittedChanges().Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.HaveCount(1)
                .And.AllBeOfType<FileHashUpdated>()
                .And.BeEquivalentTo(new FileHashUpdated(photoGuid, hash));
            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => session.Commit(ct)).MustHaveHappenedOnceExactly();
        }

        private static byte[] CreatePseudoRandomByteArray(uint size, int seed)
        {
            var random = new Random(seed);
            var result = new byte[size];
            for (var i = 0; i < size; i++)
                result[i] = (byte)random.Next(0, 255);
            return result;
        }
    }
}
