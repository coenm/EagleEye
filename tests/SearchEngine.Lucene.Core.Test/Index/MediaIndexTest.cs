namespace SearchEngine.Lucene.Core.Test.Index
{
    using System;
    using System.Threading.Tasks;

    using FluentAssertions;

    using global::Lucene.Net.Search;

    using SearchEngine.Lucene.Core.Test.Data;
    using SearchEngine.LuceneNet.Core;
    using SearchEngine.LuceneNet.Core.Index;

    using Xunit;

    public class MediaIndexTest : IDisposable
    {
        private readonly MediaIndex sut;

        public MediaIndexTest()
        {
            ILuceneDirectoryFactory indexDirectoryFactory = new RamLuceneDirectoryFactory();
            sut = new MediaIndex(indexDirectoryFactory);
        }

        public void Dispose()
        {
            sut?.Dispose();
        }

        [Fact]
        public void Dispose_ShouldNotThrowTest()
        {
            sut.Dispose();
        }

        [Fact]
        public void Search_AllDocs_ShouldReturnEmpty_WhenNothingIsIndexedTest()
        {
            // arrange

            // act
            var result = sut.Search(new MatchAllDocsQuery(), null, out var totalCount);

            // assert
            totalCount.Should().Be(0);
            result.Should().BeEmpty();
        }

        [Fact]
        public void Search_UsingWildcard_ShouldReturnEmpty_WhenNothingIsIndexedTest()
        {
            // arrange

            // act
            var result = sut.Search("a*", out var totalCount);

            // assert
            totalCount.Should().Be(0);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Index_ValidMediaObject_ShouldReturnTrueTest()
        {
            // arrange
            var data = DataStore.File001;

            // act
            var result = await sut.IndexMediaFileAsync(data).ConfigureAwait(false);

            // assert
            result.Should().BeTrue();
            sut.Count().Should().Be(1);
        }

        [Fact]
        public async Task Index_ValidMediaObjectTwice_ShouldOnlyIndexLastTest()
        {
            // arrange
            var data = DataStore.File001;

            // act
            var result1 = await sut.IndexMediaFileAsync(data).ConfigureAwait(false);
            var result2 = await sut.IndexMediaFileAsync(data).ConfigureAwait(false);

            // assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            sut.Count().Should().Be(1);
        }
    }
}