namespace EagleEye.Core.Test.CqrsLite
{
    using System;
    using System.Collections.Generic;

    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Snapshotting;
    using EagleEye.Core.CqrsLite;
    using FluentAssertions;
    using Xunit;

    public class ConfigurableSnapshotStrategyTest
    {
        private readonly Guid guid = Guid.NewGuid();

        [Fact]
        public void IsSnapshotable_ShouldReturnFalse_WhenTypeIsNotSnapshotable()
        {
            // arrange
            var sut = new ConfigurableSnapshotStrategy(5);

            // act
            var result = sut.IsSnapshotable(typeof(NotSnapshotableOnlyAggregateRootClass));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsSnapshotable_ShouldReturnTrue_WhenTypeIsSnapshotable()
        {
            // arrange
            var sut = new ConfigurableSnapshotStrategy(5);

            // act
            var result = sut.IsSnapshotable(typeof(SnapshotableClass));

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldMakeSnapShot_ShouldReturnFalse_WhenAggregateHasNoUnsavedEvents()
        {
            // arrange
            var sut = new ConfigurableSnapshotStrategy(1);
            var aggregate = new SnapshotableClass(Guid.NewGuid(), 23);

            var result = sut.ShouldMakeSnapShot(aggregate);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldMakeSnapShot_ShouldReturnTrue_WhenAggregateHasSingleUnsavedEventsAndNewVersionIsMultipleOfInterval()
        {
            // arrange
            var sut = new ConfigurableSnapshotStrategy(5);
            var aggregate = new SnapshotableClass(guid, 4, new DummyEvent(guid, 5));

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void ShouldMakeSnapShot_ShouldReturnFalse_WhenAggregateHasSingleUnsavedEventsAndNewVersionIsNotMultipleOfInterval(int currentAggregateVersion)
        {
            // arrange
            var sut = new ConfigurableSnapshotStrategy(5);
            var aggregate = new SnapshotableClass(guid, currentAggregateVersion, new DummyEvent(guid, currentAggregateVersion + 1));

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(1, 1, false)]
        [InlineData(2, 1, false)]
        [InlineData(3, 1, false)]
        [InlineData(4, 1, true)] // (4 + 1) % 5 = 0
        [InlineData(5, 1, false)]
        [InlineData(6, 1, false)]
        [InlineData(1, 3, false)] // (1 + 3) % 5 != 0
        [InlineData(1, 4, true)]
        [InlineData(1, 5, true)]
        [InlineData(1, 6, true)]
        [InlineData(1, 7, true)]
        [InlineData(3, 2, true)]
        [InlineData(3, 4, true)]
        public void ShouldMakeSnapShot_ShouldReturnExpectedValue(int currentAggregateVersion, int eventCounter, bool expectedShouldMakeSnapshot)
        {
            // arrange
            var sut = new ConfigurableSnapshotStrategy(5);
            var events = new List<IEvent>();
            for (int i = 0; i < eventCounter; i++)
            {
                events.Add(new DummyEvent(guid, currentAggregateVersion + i));
            }

            var aggregate = new SnapshotableClass(guid, currentAggregateVersion, events.ToArray());

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            result.Should().Be(expectedShouldMakeSnapshot);
        }

        private class DummyEvent : IEvent
        {
            public DummyEvent(Guid id, int version)
            {
                Id = id;
                Version = version;
            }

            public Guid Id { get; set; }

            public int Version { get; set; }

            public DateTimeOffset TimeStamp { get; set; }
        }

        private class NotSnapshotableOnlyAggregateRootClass : AggregateRoot
        {
        }

        private class SnapshotableClass : SnapshotAggregateRoot<SnapshotableClassSnapshot>
        {
            public SnapshotableClass(Guid newGuid, int version, params IEvent[] events)
            {
                Id = newGuid;
                Version = version;
                foreach (var e in events)
                    ApplyChange(e);
            }

            protected override SnapshotableClassSnapshot CreateSnapshot()
            {
                return new SnapshotableClassSnapshot();
            }

            protected override void RestoreFromSnapshot(SnapshotableClassSnapshot snapshot)
            {
            }
        }

        private class SnapshotableClassSnapshot : Snapshot
        {
        }
    }
}
