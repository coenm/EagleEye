namespace SearchEngine.LuceneCore.Test
{
    using FluentAssertions;

    using Lucene.Net.Store;

    using SearchEngine.LuceneNet.Core;

    using Xunit;

    public class RamDirectoryFactoryTest
    {
        [Fact]
        public void Create_ReturnsRAMDirectoryTest()
        {
            // arrange
            var sut = new RamDirectoryFactory();

            // act
            var result = sut.Create();

            // assert
            result.Should().BeOfType<RAMDirectory>();
        }
    }
}
