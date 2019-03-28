namespace EagleEye.Core.DefaultImplementations.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    public class InMemoryEventStore : IEventStore
    {
        private readonly IEventPublisher publisher;
        private readonly Dictionary<Guid, List<IEvent>> inMemoryDb = new Dictionary<Guid, List<IEvent>>();

        public InMemoryEventStore([NotNull] IEventPublisher publisher)
        {
            Helpers.Guards.Guard.NotNull(publisher, nameof(publisher));
            this.publisher = publisher;
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var @event in events)
            {
                inMemoryDb.TryGetValue(@event.Id, out var list);
                if (list == null)
                {
                    list = new List<IEvent>();
                    inMemoryDb.Add(@event.Id, list);
                }

                list.Add(@event);
                await publisher.Publish(@event, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            inMemoryDb.TryGetValue(aggregateId, out var events);

            return Task.FromResult(events?.Where(x => x.Version > fromVersion)
                                   ??
                                   new List<IEvent>());
        }
    }
}
