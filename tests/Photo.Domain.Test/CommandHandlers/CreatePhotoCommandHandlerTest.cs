namespace EagleEye.Photo.Domain.Test.CommandHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Domain;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.CommandHandlers.Exceptions;
    using EagleEye.Photo.Domain.Commands;
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

            // act
            await sut.Handle(new CreatePhotoCommand("a.jpg", new byte[32], "mime"), ct);

            // assert
            A.CallTo(() => uniqueFilenameService.Claim("a.jpg")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Handle_ShouldThrow_WhenFilenameAlreadyExists()
        {
            // arrange
            A.CallTo(() => uniqueFilenameService.Claim("a.jpg")).Returns(null);

            // act
            Func<Task> act = async () => await sut.Handle(new CreatePhotoCommand("a.jpg", new byte[32], "mime"), ct);

            // assert
            act.Should().Throw<PhotoAlreadyExistsException>();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenClearingPhotoHashSucceeds()
        {
            // arrange
            var token = A.Fake<IClaimFilenameToken>();
            A.CallTo(() => uniqueFilenameService.Claim("a.jpg")).Returns(token);

            // act
            await sut.Handle(new CreatePhotoCommand("a.jpg", new byte[32], "mime"), ct);

            // assert
            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => token.Commit()).MustHaveHappenedOnceExactly();
            A.CallTo(() => token.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
