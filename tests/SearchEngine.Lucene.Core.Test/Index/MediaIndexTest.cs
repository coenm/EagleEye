namespace SearchEngine.LuceneNet.Core.Test.Index
{
    using System;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Lucene.Net.Search;

    using SearchEngine.LuceneNet.Core.Index;
    using SearchEngine.LuceneNet.Core.Test.Data;

    using Xunit;

    public class MediaIndexTest : IDisposable
    {
        private readonly MediaIndex _sut;

        public MediaIndexTest()
        {
            ILuceneDirectoryFactory indexDirectoryFactory = new RamLuceneDirectoryFactory();
            _sut = new MediaIndex(indexDirectoryFactory);
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }

        [Fact]
        public void Dispose_ShouldNotThrowTest()
        {
            _sut.Dispose();
        }

        [Fact]
        public void Search_AllDocs_ShouldReturnEmpty_WhenNothingIsIndexedTest()
        {
            // arrange

            // act
            var result = _sut.Search(new MatchAllDocsQuery(), null, out var totalCount);

            // assert
            totalCount.Should().Be(0);
            result.Should().BeEmpty();
        }

        [Fact]
        public void Search_UsingWildcard_ShouldReturnEmpty_WhenNothingIsIndexedTest()
        {
            // arrange

            // act
            var result = _sut.Search("a*", out var totalCount);

            // assert
            totalCount.Should().Be(0);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Index_ValidMediaObject_ShouldReturnTrueTest()
        {
            // arrange
            var data = Datastore.File001;

            // act
            var result = await _sut.IndexMediaFileAsync(data).ConfigureAwait(false);

            // assert
            result.Should().BeTrue();
            _sut.Count().Should().Be(1);
        }

        [Fact]
        public async Task Index_ValidMediaObjectTwice_ShouldOnlyIndexLastTest()
        {
            // arrange
            var data = Datastore.File001;

            // act
            var result1 = await _sut.IndexMediaFileAsync(data).ConfigureAwait(false);
            var result2 = await _sut.IndexMediaFileAsync(data).ConfigureAwait(false);

            // assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            _sut.Count().Should().Be(1);
        }
    }
}