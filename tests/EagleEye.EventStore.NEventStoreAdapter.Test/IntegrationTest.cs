namespace EagleEye.EventStore.NEventStoreAdapter.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    /// <summary>
    /// Integration between both implementations of INEventStoreAdapterFactory and the NEventStoreAdapter.
    /// </summary>
    public class IntegrationTest
    {
        private readonly Guid aggregateId;
        private readonly IEventPublisher publisher;
        private readonly List<IEvent> publishedEvents;

        public IntegrationTest()
        {
            aggregateId = Guid.NewGuid();

            publisher = A.Fake<IEventPublisher>();
            publishedEvents = new List<IEvent>();
            A.CallTo(() => publisher.Publish(A<IEvent>._, A<CancellationToken>._))
             .Invokes(call =>
                      {
                          if (call.Arguments[0] is IEvent e)
                              publishedEvents.Add(e);
                      });
        }

        [Theory]
        [MemberData(nameof(GetContexts))]
        public async Task Save_ShouldPublishSavedEvents(Context context)
        {
            // arrange
            var eventStore = context.Factory.Create(publisher);

            var timestamp = DateTimeOffset.Now;
            var ct = new CancellationToken();
            var eventsToStore = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 1, timestamp),
                    DummyEvent.Create(aggregateId, 2, timestamp),
                    DummyEvent.Create(aggregateId, 3, timestamp),
                    DummyEvent.Create(aggregateId, 4, timestamp),
                    DummyEvent.Create(aggregateId, 5, timestamp),
                    DummyEvent.Create(aggregateId, 6, timestamp),
                };

            // act
            await eventStore.Save(eventsToStore, ct).ConfigureAwait(false);

            // assert
            var expectedEvents = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 1, timestamp),
                    DummyEvent.Create(aggregateId, 2, timestamp),
                    DummyEvent.Create(aggregateId, 3, timestamp),
                    DummyEvent.Create(aggregateId, 4, timestamp),
                    DummyEvent.Create(aggregateId, 5, timestamp),
                    DummyEvent.Create(aggregateId, 6, timestamp),
                };
            publishedEvents.Should().BeEquivalentTo(expectedEvents);
        }

        [Theory]
        [MemberData(nameof(GetContexts))]
        public async Task Get_ShouldReturnSavedItems(Context context)
        {
            // arrange
            var eventStore = context.Factory.Create(publisher);

            var timestamp = DateTimeOffset.Now;
            var ct = new CancellationToken();
            var eventsToStore = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 1, timestamp),
                    DummyEvent.Create(aggregateId, 2, timestamp),
                    DummyEvent.Create(aggregateId, 3, timestamp),
                    DummyEvent.Create(aggregateId, 4, timestamp),
                    DummyEvent.Create(aggregateId, 5, timestamp),
                    DummyEvent.Create(aggregateId, 6, timestamp),
                };

            await eventStore.Save(eventsToStore, ct).ConfigureAwait(false);

            // act
            var result = await eventStore.Get(aggregateId, 4, ct).ConfigureAwait(false);

            // assert
            var expectedEvents = new List<IEvent>
                {
                    DummyEvent.Create(aggregateId, 5, timestamp),
                    DummyEvent.Create(aggregateId, 6, timestamp),
                };
            result.Should().BeEquivalentTo(expectedEvents);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<object[]> GetContexts()
        {
            yield return new object[] { new Context(new NEventStoreAdapterInMemoryFactory(), "InMemory") };
            yield return new object[] { new Context(new NEventStoreAdapterSqliteFactory("InMemory"), "Sqlite (InMemory)") };
        }

        public struct Context
        {
            private readonly string name;

            public Context(INEventStoreAdapterFactory factory, string name)
            {
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                this.name = name ?? throw new ArgumentNullException(nameof(name));
            }

            public INEventStoreAdapterFactory Factory { get; }

            public override string ToString() => name;
        }
    }
}
