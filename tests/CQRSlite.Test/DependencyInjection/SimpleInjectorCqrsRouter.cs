namespace CQRSlite.Test.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Events;
    using CQRSlite.Queries;
    using Dawn;
    using JetBrains.Annotations;
    using SimpleInjector;

    public class SimpleInjectorCqrsRouter : ICommandSender, IEventPublisher, IQueryProcessor
    {
        private readonly Container container;

        public SimpleInjectorCqrsRouter([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            this.container = container;
        }

        public Task Send<T>(T command, CancellationToken cancellationToken = default)
            where T : class, ICommand
        {
            var commandType = command.GetType();

            var normalHandlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            var normalHandler = container.GetInstance(normalHandlerType);
            var normalHandler2 = normalHandler as ICommandHandler<T>;

            var cancellableCommandHandlerType = typeof(ICancellableCommandHandler<>).MakeGenericType(commandType);
            dynamic x2 = container.GetInstance(cancellableCommandHandlerType);
            var x21 = x2 as ICancellableCommandHandler<T>;

            if (normalHandler2 != null && x21 != null)
                throw new InvalidOperationException($"Cannot send to more than one handler of {commandType.FullName}");

            if (normalHandler2 != null)
                return normalHandler2.Handle(command);

            if (x21 != null)
                return x21.Handle(command, cancellationToken);

            throw new InvalidOperationException($"No handler registered for {commandType.FullName}");
        }

        public Task Publish<T>(T @event, CancellationToken cancellationToken = default)
            where T : class, IEvent
        {
            var eventType = @event.GetType();

            var normalHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            IEnumerable<object> normalHandler = container.GetAllInstances(normalHandlerType);
            var normalHandlers2 = new List<IEventHandler<T>>(normalHandler.Count());
            foreach (var h in normalHandler)
            {
                if (h is IEventHandler<T> h2)
                    normalHandlers2.Add(h2);
            }

            if (!normalHandlers2.Any())
                return Task.FromResult(0);

            var tasks = new Task[normalHandlers2.Count];
            for (var index = 0; index < normalHandlers2.Count; index++)
            {
                tasks[index] = normalHandlers2[index].Handle(@event);
            }

            return Task.WhenAll(tasks);
        }

        public Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
