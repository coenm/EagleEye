namespace SearchEngine.LuceneCore.Test.Index
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Lucene.Net.Search;

    using SearchEngine.LuceneNet.Core;
    using SearchEngine.LuceneNet.Core.Commands.UpdateIndex;
    using SearchEngine.LuceneNet.Core.Index;

    using Xunit;

    public class MediaIndexTest : IDisposable
    {
        private readonly MediaIndex _sut;

        public MediaIndexTest()
        {
            ILuceneDirectoryFactory indexDirectoryFactory = new RamDirectoryFactory();
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
            var data = new MediaObject
                           {
                               DateTimeTaken = new Timestamp
                                                   {
                                                       Value = new DateTime(2001, 4, 6, 12, 0, 0),
                                                       Precision = TimestampPrecision.Month,
                                                   },
                               Location = new Location
                                   {
                                       City = "New York",
                                       State = "New York",
                                       CountryName = "United States of America",
                                       SubLocation = "Ground zero",
                                       CountryCode = "USA",
                                       Coordinate = new Coordinate()
                                   },
                               Persons = new List<string>
                                             {
                                                 "Alice",
                                                 "Bob"
                                             },
                               Tags = new List<string>
                                          {
                                              "Vacation",
                                              "Summer"
                                          },
                               FileInformation = new FileInformation
                                                     {
                                                         Type = "image/jpeg",
                                                         Filename = "a/b/c/file.jpg"
                                                     }
                           };

            // act
            var result = await _sut.IndexMediaFileAsync(data).ConfigureAwait(false);

            // assert
            result.Should().BeTrue();
        }
    }
}