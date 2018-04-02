namespace SearchEngine.LuceneCore.Test.Index
{
    using System;
    using System.Collections.Generic;

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
            result.Should().BeEquivalentTo(Datastore.MediaResult001(1));
        }

        [Theory]
        [InlineData("new")] // Should match "New York" in city.
        [InlineData("city:new")] // Should match "New York" in city.
        [InlineData("date:[2001 TO 2002]")] // date is within range
        [InlineData("city:new AND date:[2001 TO 2002]")] // date is within range
        [InlineData("city:new OR date:[200110 TO 2002]")] // date is NOT within range but city:new is true
        public void Search_ShouldReturnDataTest(string searchQuery)
        {
            // arrange

            // act
            var result = _sut.Search(searchQuery, out var totalCount);
            RemoveScore(result);

            // assert
            totalCount.Should().Be(1);
            result.Should().BeEquivalentTo(Datastore.MediaResult001(0));
        }

        [Theory]
        [InlineData("date:[20010401 TO 2002]")] // date (2001-04-00) is not witin range
        [InlineData("city:N* AND date:[20010401 TO 2002]")] // date is not within range
        public void Search_WithinWrongeDateRange_ShouldReturnEmptyTest(string searchQuery)
        {
            // arrange

            // act
            var result = _sut.Search(searchQuery, out var totalCount);

            // assert
            totalCount.Should().Be(0);
            result.Should().BeEmpty();
        }


        private static void RemoveScore(List<MediaResult> result)
        {
            result.ForEach(x => x.Score = 0);
        }
    }
}