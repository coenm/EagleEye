namespace Photo.ReadModel.SearchEngineLucene.Test.Index
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using FluentAssertions;
    using Lucene.Net.Search;
    using Photo.ReadModel.SearchEngineLucene.Test.Data;
    using Xunit;

    public class PhotoIndexTest : IDisposable
    {
        private readonly PhotoIndex sut;

        public PhotoIndexTest()
        {
            ILuceneDirectoryFactory indexDirectoryFactory = new RamLuceneDirectoryFactory();
            sut = new PhotoIndex(indexDirectoryFactory);
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
            var result = await sut.ReIndexMediaFileAsync(data).ConfigureAwait(false);

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
            var result1 = await sut.ReIndexMediaFileAsync(data).ConfigureAwait(false);
            var result2 = await sut.ReIndexMediaFileAsync(data).ConfigureAwait(false);

            // assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            sut.Count().Should().Be(1);
        }
    }
}
