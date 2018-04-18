namespace SearchEngine.Lucene.Core.Test
{
    using FluentAssertions;

    using global::Lucene.Net.Store;

    using SearchEngine.LuceneNet.Core;

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
