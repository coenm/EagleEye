namespace Photo.ReadModel.SearchEngineLucene.Test.Index
{
    using System;

    using FluentAssertions;
    using Lucene.Net.Search;
    using Photo.ReadModel.SearchEngineLucene.Interface;
    using Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories;
    using Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using Photo.ReadModel.SearchEngineLucene.Test.Data;
    using Xunit;

    public class MediaIndexSearchTest : IDisposable
    {
        private readonly PhotoIndex sut;

        public MediaIndexSearchTest()
        {
            ILuceneDirectoryFactory indexDirectoryFactory = new RamLuceneDirectoryFactory();
            sut = new PhotoIndex(indexDirectoryFactory);

            var data = DataStore.File001;
            sut.ReIndexMediaFileAsync(data).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            sut?.Dispose();
        }

        [Fact]
        public void Search_AllDocs_ShouldReturnEmpty_WhenNothingIsIndexedTest()
        {
            // arrange

            // act
            var result = sut.Search(new MatchAllDocsQuery(), null, out var totalCount);

            // assert
            totalCount.Should().Be(1);
            result.Should().BeEquivalentTo(DataStore.MediaResult001(1));
        }

        [Theory]
        [InlineData("new", 0.03345561F)] // Should match "New York" in city.
        [InlineData("city:new", 0.191783011F)] // Should match "New York" in city.
        [InlineData("date:[2001 TO 2002]", 1F)] // date is within range
        [InlineData("city:new AND date:[2001 TO 2002]", 1.01226437F)] // date is within range
        [InlineData("city:new OR date:[200110 TO 2002]", 0.0281300247F)] // date is NOT within range but city:new is true
        public void Search_ShouldReturnDataTest(string searchQuery, float expectedScore)
        {
            // arrange

            // act
            var result = sut.Search(searchQuery, out var totalCount);

            // assert
            totalCount.Should().Be(1);
            result.Should().BeEquivalentTo(DataStore.MediaResult001(expectedScore));
        }

        [Theory]
        [InlineData("date:[20010401 TO 2002]")] // date (2001-04-00) is not within range
        [InlineData("city:N* AND date:[20010401 TO 2002]")] // date is not within range
        public void Search_WithinWrongDateRange_ShouldReturnEmptyTest(string searchQuery)
        {
            // arrange

            // act
            var result = sut.Search(searchQuery, out var totalCount);

            // assert
            totalCount.Should().Be(0);
            result.Should().BeEmpty();
        }
    }
}
