namespace NEventStore.Test
{
    using System;

    using FluentAssertions;
    using NEventStore.Serialization.Json;
    using Xunit;

    public class PocNEventStore
    {
        [Fact]
        public void WriteAndReadSingleStreamTest()
        {
            // arrange
            var aggregateId = Guid.NewGuid();
            IStoreEvents store = Wireup.Init()
                              .UseOptimisticPipelineHook()
                              .UsingInMemoryPersistence()
                              .InitializeStorageEngine()
                              .UsingJsonSerialization()
                              .HookIntoPipelineUsing(new DummyPipelineHook())
                              .Build();

            // act
            using (store)
            {
                using (var stream = store.OpenStream(aggregateId))
                {
                    stream.Add(new EventMessage { Body = CreateDummyEvent(aggregateId, "a"), });
                    stream.Add(new EventMessage { Body = CreateDummyEvent(aggregateId, "b"), });

                    // not sure yet what it means to have a commit id.
                    stream.CommitChanges(Guid.NewGuid());
                }

                using (var stream = store.OpenStream(aggregateId, 0, int.MaxValue))
                {
                    // assert
                    stream.StreamId.Should().Be(aggregateId.ToString(), "Because the aggregateId was the stream identifier");
                    stream.BucketId.Should().Be("default", "we didn't supply a bucket id.");
                    stream.StreamRevision.Should().Be(2, "there are two events committed");
                    stream.CommitSequence.Should().Be(1, "Only one commit has taken place");
                    stream.CommittedEvents
                          .Should()
                          .BeEquivalentTo(new object[]
                                          {
                                              new EventMessage { Body = CreateDummyEvent(aggregateId, "a"), },
                                              new EventMessage { Body = CreateDummyEvent(aggregateId, "b"), },
                                          });
                }

                using (var stream = store.OpenStream(aggregateId, 2, int.MaxValue))
                {
                    // assert
                    stream.StreamId.Should().Be(aggregateId.ToString(), "Because the aggregateId was the stream identifier");
                    stream.BucketId.Should().Be("default", "we didn't supply a bucket id.");
                    stream.StreamRevision.Should().Be(2, "there are two events committed");
                    stream.CommitSequence.Should().Be(1, "Only one commit has taken place");
                    stream.CommittedEvents
                          .Should()
                          .BeEquivalentTo(new object[]
                                          {
                                              new EventMessage { Body = CreateDummyEvent(aggregateId, "b"), },
                                          });
                }
            }
        }

        private static DummyEvent CreateDummyEvent(Guid id, string name)
        {
            return new DummyEvent
                   {
                       Id = id,
                       OriginalName = name + "orig",
                       NewName = name + "new",
                   };
        }

        private class DummyEvent
        {
            public Guid Id { get; set; }

            public string OriginalName { get; set; }

            public string NewName { get; set; }
        }

        private class DummyPipelineHook : IPipelineHook
        {
            public void Dispose()
            {
            }

            public ICommit Select(ICommit committed)
            {
                return committed;
            }

            public bool PreCommit(CommitAttempt attempt)
            {
                return true;
            }

            public void PostCommit(ICommit committed)
            {
            }

            public void OnPurge(string bucketId)
            {
                return;
            }

            public void OnDeleteStream(string bucketId, string streamId)
            {
            }
        }
    }
}
