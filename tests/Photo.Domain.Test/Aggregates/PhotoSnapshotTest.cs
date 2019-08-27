namespace EagleEye.Photo.Domain.Test.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Snapshotting;
    using EagleEye.Core.CqrsLite;
    using EagleEye.Core.DefaultImplementations.EventStore;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Aggregates.SnapshotDtos;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class PhotoSnapshotTest
    {
        private readonly Guid guid;
        private readonly string filename;
        private readonly string mimeType;
        private readonly byte[] fileSha;
        private readonly CancellationToken token;
        private readonly InMemoryEventStore eventStore;

        public PhotoSnapshotTest()
        {
            guid = Guid.NewGuid();
            filename = "c:/a/b/c.jpg";
            mimeType = "image/jpg";
            fileSha = new byte[32];

            token = CancellationToken.None;
            eventStore = new InMemoryEventStore(A.Dummy<IEventPublisher>());
        }

        [Fact]
        public async Task PhotoSnapshotCreationTest()
        {
            // arrange
            var snapshotStore = A.Fake<ISnapshotStore>();
            var savedSnapshots = new List<Snapshot>();

            A.CallTo(() => snapshotStore.Save(A<Snapshot>._, token))
                .Invokes(call =>
                {
                    savedSnapshots.Add(call.Arguments[0] as Snapshot);
                });
            var snapshotStrategy = new ConfigurableSnapshotStrategy(1);
            var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(snapshotRepository);
            var photo = new Photo(guid, filename, mimeType, fileSha);

            // act
            await session.Add(photo, token);
            await session.Commit(token);

            // assert
            A.CallTo(() => snapshotStore.Save(A<Snapshot>._, token)).MustHaveHappenedOnceExactly();
            A.CallTo(snapshotStore).MustHaveHappenedOnceExactly();
            savedSnapshots.Should().HaveCount(1).And.AllBeOfType<PhotoAggregateSnapshot>();
            var expectedSnapshot = new PhotoAggregateSnapshot
            {
                Id = guid,
                Filename = filename,
                FileHash = fileSha,
                Version = 1,
                PhotoHashes = new Dictionary<string, ulong>(0),
                Location = null,
                Persons = new List<string>(0),
                Tags = new List<string>(0),
                DateTimeTaken = null,
            };
            savedSnapshots.Single().Should().BeEquivalentTo(expectedSnapshot);
        }
    }
}
