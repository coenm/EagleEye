namespace EagleEye.EventStore.NEventStoreAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using JetBrains.Annotations;
    using NEventStore;

    internal class NEventStoreEventExporter : IEventExporter
    {
        [NotNull] private readonly IStoreEvents store;

        public NEventStoreEventExporter(NEventStore.IStoreEvents store)
        {
            Guard.Argument(store, nameof(store)).NotNull();

            this.store = store;
        }

        public Task<IEnumerable<IEvent>> GetAsync(DateTime from, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult((IEnumerable<IEvent>)store.Advanced
                                                             .GetFrom(Bucket.Default, from)
                                                             .SelectMany(x => x.Events)
                                                             .Select(x => x.Body as IEvent)
                                                             .Where(x => x != null)
                                                             .ToArray());
        }
    }
}
