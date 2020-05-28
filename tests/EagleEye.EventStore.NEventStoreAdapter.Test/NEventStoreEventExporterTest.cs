namespace EagleEye.EventStore.NEventStoreAdapter.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using FakeItEasy;
    using FluentAssertions;
    using NEventStore;
    using NEventStore.Persistence;
    using Xunit;

    public class NEventStoreEventExporterTest
    {
        private readonly NEventStoreEventExporter sut;
        private readonly IPersistStreams persistStreams;
        private readonly Guid aggregateId;

        public NEventStoreEventExporterTest()
        {
            aggregateId = Guid.NewGuid();
            var store = A.Fake<IStoreEvents>();
            persistStreams = A.Fake<IPersistStreams>();
            A.CallTo(() => store.Advanced).Returns(persistStreams);
            sut = new NEventStoreEventExporter(store);
        }

        [Fact]
        public async Task GetAsync_ShouldCallPersistStreamsGetFrom()
        {
            // arrange
            var from = DateTime.MinValue;

            // act
            _ = await sut.GetAsync(from, CancellationToken.None);

            // assert
            A.CallTo(() => persistStreams.GetFrom(Bucket.Default, from)).MustHaveHappenedOnceExactly();
            A.CallTo(persistStreams).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEventsFromStreamAfterTimestamp()
        {
            // arrange
            var from = DateTime.Now;

            var commits = new List<ICommit>
                          {
                                CreateCommit(
                                             new EventMessage { Body = DummyEvent.Create(aggregateId, 1, from), },
                                             new EventMessage { Body = DummyEvent.Create(aggregateId, 2, from), }),
                                CreateCommit(new EventMessage { Body = DummyEvent.Create(aggregateId, 3, from), }),
                                CreateCommit(new EventMessage { Body = "not an IEvent" }),
                                CreateCommit(new EventMessage { Body = DummyEvent.Create(aggregateId, 5, from), }),
                                CreateCommit(new EventMessage { Body = DummyEvent.Create(aggregateId, 4, from), }),
                          };

            A.CallTo(() => persistStreams.GetFrom(Bucket.Default, from)).Returns(commits);

            // act
            var result = await sut.GetAsync(from, CancellationToken.None);

            // assert
            var expectedEvents = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 1, from),
                    DummyEvent.Create(aggregateId, 2, from),
                    DummyEvent.Create(aggregateId, 3, from),
                    DummyEvent.Create(aggregateId, 4, from),
                    DummyEvent.Create(aggregateId, 5, from),
                };
            result.Should().BeEquivalentTo(expectedEvents);
        }

        private static ICommit CreateCommit(params EventMessage[] eventMessage)
        {
            var commit = A.Fake<ICommit>();
            A.CallTo(() => commit.Events).Returns(eventMessage);
            return commit;
        }
    }
}
