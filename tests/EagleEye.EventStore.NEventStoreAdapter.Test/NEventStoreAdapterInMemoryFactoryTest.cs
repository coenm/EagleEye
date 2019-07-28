namespace EagleEye.EventStore.NEventStoreAdapter.Test
{
    using System;

    using CQRSlite.Events;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class NEventStoreAdapterInMemoryFactoryTest
    {
        private readonly NEventStoreAdapterInMemoryFactory sut;

        public NEventStoreAdapterInMemoryFactoryTest()
        {
            sut = new NEventStoreAdapterInMemoryFactory();
        }

        [Fact]
        public void Create_ShouldReturnNEventStoreAdapterInstance()
        {
            // arrange
            var publisher = A.Dummy<IEventPublisher>();

            // act
            var result = sut.Create(publisher);

            // assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NEventStoreAdapter>();
        }

        [Fact]
        public void Dispose()
        {
            // arrange

            // act
            Action act = () => sut.Dispose();

            // assert
            act.Should().NotThrow();
        }
    }
}
