namespace EagleEye.Photo.Domain.Decorators
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class VerifyTokenCommandHandlerDecorator<T>
        : ICancellableCommandHandler<T>
        where T : ICommand
    {
        [NotNull] private readonly ICancellableCommandHandler<T> decoratee;

        public VerifyTokenCommandHandlerDecorator([NotNull] ICancellableCommandHandler<T> decoratee)
        {
            Dawn.Guard.Argument(decoratee, nameof(decoratee)).NotNull();
            this.decoratee = decoratee;
        }

        public Task Handle(T message, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            return decoratee.Handle(message, token);
        }
    }
}
