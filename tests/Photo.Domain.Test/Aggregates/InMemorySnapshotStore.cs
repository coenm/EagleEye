namespace EagleEye.Photo.Domain.Test.Aggregates
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Snapshotting;

    internal class InMemorySnapshotStore : ISnapshotStore
    {
        private readonly ConcurrentDictionary<Guid, Snapshot> inMemoryDb = new ConcurrentDictionary<Guid, Snapshot>();
        private readonly List<Snapshot> savedSnapshots = new List<Snapshot>();
        private readonly List<Guid> requestedSnapshots = new List<Guid>();

        public ReadOnlyCollection<Snapshot> SavedSnapshots => savedSnapshots.AsReadOnly();

        public ReadOnlyCollection<Guid> RequestedSnapshots => requestedSnapshots.AsReadOnly();

        public Task<Snapshot> Get(Guid id, CancellationToken cancellationToken = default)
        {
            requestedSnapshots.Add(id);
            if (inMemoryDb.TryGetValue(id, out var item))
                return Task.FromResult(item);
            return Task.FromResult(null as Snapshot);
        }

        public Task Save(Snapshot snapshot, CancellationToken cancellationToken = default)
        {
            savedSnapshots.Add(snapshot);
            inMemoryDb.AddOrUpdate(snapshot.Id, _ => snapshot, (_, __) => snapshot);
            return Task.CompletedTask;
        }
    }
}
