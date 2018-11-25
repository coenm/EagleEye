namespace SearchEngine.Lucene.ReadModel.Test
{
    using FluentAssertions;
    using global::Lucene.Net.Store;
    using SearchEngine.LuceneNet.ReadModel.Internal.LuceneDirectoryFactories;
    using Xunit;

    public class RamDirectoryFactoryTest
    {
        [Fact]
        public void Create_ReturnsRAMDirectoryTest()
        {
            // arrange
            var sut = new RamLuceneDirectoryFactory();

            // act
            var result = sut.Create();

            // assert
            result.Should().BeOfType<RAMDirectory>();
        }
    }
}
