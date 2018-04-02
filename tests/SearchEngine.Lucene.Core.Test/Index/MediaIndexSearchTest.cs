namespace SearchEngine.LuceneCore.Test.Index
{
    using System;

    using FluentAssertions;

    using Lucene.Net.Search;

    using SearchEngine.LuceneCore.Test.Data;
    using SearchEngine.LuceneNet.Core;
    using SearchEngine.LuceneNet.Core.Index;

    using Xunit;

    public class MediaIndexSearchTest : IDisposable
    {
        private readonly MediaIndex _sut;

        public MediaIndexSearchTest()
        {
            ILuceneDirectoryFactory indexDirectoryFactory = new RamDirectoryFactory();
            _sut = new MediaIndex(indexDirectoryFactory);

            var data = Datastore.File001;
            _sut.IndexMediaFileAsync(data).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }

        [Fact]
        public void Search_AllDocs_ShouldReturnEmpty_WhenNothingIsIndexedTest()
        {
            // arrange

            // act
            var result = _sut.Search(new MatchAllDocsQuery(), null, out var totalCount);

            // assert
            totalCount.Should().Be(1);
            result.Should().BeEquivalentTo(Datastore.MediaResult001);
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
    }
}