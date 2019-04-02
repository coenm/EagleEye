namespace EagleEye.Photo.Domain.Decorators
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using Dawn;
    using JetBrains.Annotations;

    internal class VerifyTokenCommandHandlerDecorator<T>
        : ICancellableCommandHandler<T>
        where T : ICommand
    {
        [NotNull] private readonly ICancellableCommandHandler<T> decoratee;

        public VerifyTokenCommandHandlerDecorator([NotNull] ICancellableCommandHandler<T> decoratee)
        {
            Guard.Argument(decoratee, nameof(decoratee)).NotNull();
            this.decoratee = decoratee;
        }

        public Task Handle(T message, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return decoratee.Handle(message, token);
        }
    }
}
