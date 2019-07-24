namespace EagleEye.EventStore.NEventStoreAdapter.Test
{
    using CQRSlite.Events;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class NEventStoreAdapterSqliteFactoryTest
    {
        private readonly NEventStoreAdapterSqliteFactory sut;

        public NEventStoreAdapterSqliteFactoryTest()
        {
            sut = new NEventStoreAdapterSqliteFactory("InMemory");
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
    }
}
