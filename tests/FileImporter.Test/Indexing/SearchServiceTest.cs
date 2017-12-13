using System;
using System.Collections.Generic;
using FakeItEasy;
using FileImporter.Indexing;
using Xunit;

namespace FileImporter.Test.Indexing
{
    public class SearchServiceTest
    {
        private readonly IFileIndexRepository _repository;

        public SearchServiceTest()
        {
            _repository = A.Fake<IFileIndexRepository>();
        }

        [Fact]
        public void CtorDoesNotThrowTest()
        {
            // arrange

            // act
            var sut = new SearchService(_repository);

            // assert
            Assert.NotNull(sut);
        }

        [Fact]
        public void CtorThrowsOnNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new SearchService(null));
        }

        [Fact]
        public void FindSimilarCallsRepositoryAndReturnsItResultTest()
        {
            // arrange
            var sut = new SearchService(_repository);
            var src = A.Dummy<FileIndex>();
            A.CallTo(() => _repository.FindSimilar(src, 1, 2, 3, 4, 5)).Returns(new List<FileIndex>());

            // act
            var result = sut.FindSimilar(src, 1, 2, 3, 4, 5);

            // assert
            A.CallTo(() => _repository.FindSimilar(src, 1, 2, 3, 4, 5)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.Equal(new List<FileIndex>(), result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetCallsRepositoryAndReturnsItResultTest(bool resultFound)
        {
            // arrange
            var sut = new SearchService(_repository);
            var id = A.Dummy<string>();
            var fileIndex = resultFound ? new FileIndex("") : null;
            A.CallTo(() => _repository.Get(id)).Returns(fileIndex);

            // act
            var result = sut.FindById(id);

            // assert
            A.CallTo(() => _repository.Get(id)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.Equal(fileIndex, result);
        }

    }
}