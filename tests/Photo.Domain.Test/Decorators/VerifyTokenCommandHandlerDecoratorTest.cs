namespace EagleEye.Photo.Domain.Test.Decorators
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using EagleEye.Photo.Domain.Decorators;
    using FakeItEasy;
    using FluentAssertions;
    using JetBrains.Annotations;
    using Xunit;

    public class VerifyTokenCommandHandlerDecoratorTest
    {
        private readonly CancellationToken ct;
        private readonly DummyCommand message;

        public VerifyTokenCommandHandlerDecoratorTest()
        {
            ct = new CancellationToken(false);
            message = A.Dummy<DummyCommand>();
        }

        [Fact]
        public async Task Handle_ShouldPassDataToDecorator_WhenTokenIsNotCancelled()
        {
            // arrange
            var decoratee = A.Fake<ICancellableCommandHandler<DummyCommand>>();
            var sut = new VerifyTokenCommandHandlerDecorator<DummyCommand>(decoratee);

            // act
            await sut.Handle(message, ct);

            // assert
            A.CallTo(() => decoratee.Handle(message, ct)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Handle_ShouldThrow_WhenTokenIsCancelled()
        {
            // arrange
            var decoratee = A.Fake<ICancellableCommandHandler<DummyCommand>>();
            var sut = new VerifyTokenCommandHandlerDecorator<DummyCommand>(decoratee);

            // act
            Func<Task> act = async () => await sut.Handle(message, new CancellationToken(true));

            // assert
            act.Should().Throw<OperationCanceledException>();
            A.CallTo(decoratee).MustNotHaveHappened();
        }

        [UsedImplicitly]
        public class DummyCommand : ICommand
        {
        }
    }
}
