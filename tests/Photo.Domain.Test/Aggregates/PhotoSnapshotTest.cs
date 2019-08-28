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
            var snapshotStore = new InMemorySnapshotStore();
            var snapshotStrategy = new ConfigurableSnapshotStrategy(1);
            var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(snapshotRepository);
            var photo = new Photo(guid, filename, mimeType, fileSha);

            // act
            await session.Add(photo, token);
            await session.Commit(token);

            // assert
            snapshotStore.SavedSnapshots.Should().HaveCount(1).And.AllBeOfType<PhotoAggregateSnapshot>();
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
            snapshotStore.SavedSnapshots.Single().Should().BeEquivalentTo(expectedSnapshot);
            snapshotStore.RequestedSnapshots.Should().BeEmpty();
        }

        [Fact]
        public async Task PhotoSnapshotRestoresPhotoTest()
        {
            // arrange
            var snapshotStore = new InMemorySnapshotStore();
            var snapshotStrategy = new ConfigurableSnapshotStrategy(1);
            var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(snapshotRepository);
            var photo = new Photo(guid, filename, mimeType, fileSha);
            await session.Add(photo, token);
            await session.Commit(token);

            // act
            var photo2 = await session.Get<Photo>(guid, cancellationToken: token);

            // assert
            photo.Should().BeEquivalentTo(photo2);
            snapshotStore.RequestedSnapshots.Should().BeEquivalentTo(new Guid[] { guid });
        }

        [Fact]
        public async Task PhotoSnapshotSaveAndRestoresPhotoTest()
        {
            // arrange
            var snapshotStore = new InMemorySnapshotStore();
            var snapshotStrategy = new ConfigurableSnapshotStrategy(1);
            var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(snapshotRepository);

            // act
            var photo = new Photo(guid, filename, mimeType, fileSha);
            await session.Add(photo, token);
            await session.Commit(token);

            photo = await session.Get<Photo>(guid, cancellationToken: token);
            photo.SetLocation("NL", "Netherlands", "X", "Y", "Z", null, null);
            await session.Commit(token);

            photo = await session.Get<Photo>(guid, cancellationToken: token);
            photo.SetLocation("NL", "Netherlands", "X1", "Y1", "Z1", 162.99F, 88.4F);
            await session.Commit(token);

            photo = await session.Get<Photo>(guid, cancellationToken: token);
            photo.UpdatePhotoHash("AverageHash", 83492346394UL);
            await session.Commit(token);

            photo = await session.Get<Photo>(guid, cancellationToken: token);
            photo.UpdatePhotoHash("DummyHash", 123UL);
            await session.Commit(token);

            // assert
            var photo2 = await session.Get<Photo>(guid, cancellationToken: token);
            photo.Should().BeEquivalentTo(photo2);
            snapshotStore.RequestedSnapshots.Should().BeEquivalentTo(new Guid[] { guid, guid, guid, guid, guid });
        }
    }
}
