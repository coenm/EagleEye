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
    using Xunit;

    public class NEventStoreAdapterTest
    {
        private readonly NEventStoreAdapter sut;
        private readonly IEventPublisher publisher;
        private readonly IStoreEvents store;
        private readonly IEventStream eventSteam;
        private readonly Guid aggregateId;

        public NEventStoreAdapterTest()
        {
            aggregateId = Guid.NewGuid();
            publisher = A.Fake<IEventPublisher>();
            store = A.Fake<IStoreEvents>();
            eventSteam = A.Fake<IEventStream>();
            A.CallTo(() => store.OpenStream(Bucket.Default, aggregateId.ToString(), A<int>._, A<int>._))
                .Returns(eventSteam);
            A.CallTo(() => eventSteam.CommittedEvents).Returns(new List<EventMessage>(0));
            sut = new NEventStoreAdapter(publisher, store);
        }

        [Fact]
        public async Task Get_ShouldOpenStreamFromStoreAndReadCommittedEvents()
        {
            // arrange
            const int fromVersion = 16;

            // act
            _ = await sut.Get(aggregateId, fromVersion, CancellationToken.None);

            // assert
            A.CallTo(() => store.OpenStream(Bucket.Default, aggregateId.ToString(), fromVersion, int.MaxValue))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => eventSteam.CommittedEvents).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Get_ShouldReturnEventsFromStream()
        {
            // arrange
            const int fromVersion = 2;
            var timestamp = DateTimeOffset.Now;

            var events = new List<EventMessage>
                {
                    new EventMessage { Body = DummyEvent.Create(aggregateId, 1, timestamp), },
                    new EventMessage { Body = DummyEvent.Create(aggregateId, 2, timestamp), },
                    new EventMessage { Body = DummyEvent.Create(aggregateId, 3, timestamp), },
                    new EventMessage { Body = "not an IEvent" },
                    new EventMessage { Body = DummyEvent.Create(aggregateId, 5, timestamp), },
                    new EventMessage { Body = DummyEvent.Create(aggregateId, 4, timestamp), },
                };
            A.CallTo(() => eventSteam.CommittedEvents).Returns(events);

            // act
            var result = await sut.Get(aggregateId, fromVersion, CancellationToken.None);

            // assert
            var expectedEvents = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 3, timestamp),
                    DummyEvent.Create(aggregateId, 4, timestamp),
                    DummyEvent.Create(aggregateId, 5, timestamp),
                };
            result.Should().BeEquivalentTo(expectedEvents);
        }

        [Fact]
        public async Task Save_ShouldSaveEventToStreamAndPublishEvent()
        {
            // arrange
            var timestamp = DateTimeOffset.Now;
            var ct = new CancellationToken();
            var eventsToStore = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 3, timestamp),
                };

            // act
            await sut.Save(eventsToStore, ct);

            // assert
            A.CallTo(() => store.OpenStream(Bucket.Default, aggregateId.ToString(), int.MinValue, int.MaxValue))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSteam.Add(A<EventMessage>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => eventSteam.Add(A<EventMessage>.That.Matches(e => e.Body.Equals(eventsToStore[0]))))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSteam.CommitChanges(A<Guid>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => publisher.Publish(eventsToStore[0], ct)).MustHaveHappenedOnceExactly();
        }
    }
}
