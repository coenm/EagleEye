namespace EagleEye.Photo.Domain.Test.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    public class SetDateTimeTakenCommandHandlerTest
    {
        [NotNull] private readonly SetDateTimeTakenCommandHandler sut;
        [NotNull] private readonly ISession session;
        private readonly Guid photoGuid;
        private readonly CancellationToken ct;

        public SetDateTimeTakenCommandHandlerTest()
        {
            session = A.Fake<ISession>();
            sut = new SetDateTimeTakenCommandHandler(session);
            photoGuid = Guid.NewGuid();
            ct = default;
        }

        [Fact]
        public async Task Handle_ShouldGetAggregateFromSession()
        {
            // arrange
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(new Photo(photoGuid, "dummy", "dummy2", new byte[32]));
            var dateTimeTaken = new Commands.Inner.Timestamp(2012, 11, 24);

            // act
            await sut.Handle(new SetDateTimeTakenCommand(photoGuid, 42, dateTimeTaken), ct);

            // assert
            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhotoAggregateAndCommitPhotoToSession_WhenUpdatingPhotoHashSucceeds()
        {
            // arrange
            var dateTimeTaken = new Commands.Inner.Timestamp(2012, 11, 24);
            var photo = new Photo(photoGuid, "dummy", "dummy2", new byte[32]);
            photo.FlushUncommittedChanges();

            A.CallTo(() => session.Get<Photo>(photoGuid, 42, ct))
                .Returns(photo);

            // act
            await sut.Handle(new SetDateTimeTakenCommand(photoGuid, 42, dateTimeTaken), ct);

            // assert
            photo.GetUncommittedChanges().Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.HaveCount(1)
                .And.AllBeOfType<DateTimeTakenChanged>()
                .And.BeEquivalentTo(new DateTimeTakenChanged(photoGuid, new DateTime(2012, 11, 24), TimestampPrecision.Day));
            A.CallTo(() => session.Add(A<Photo>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => session.Commit(ct)).MustHaveHappenedOnceExactly();
        }
    }
}
