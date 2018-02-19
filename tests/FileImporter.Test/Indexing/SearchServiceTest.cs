namespace EagleEye.FileImporter.Test.Indexing
{
    using System;
    using System.Collections.Generic;

    using EagleEye.FileImporter.Indexing;

    using FakeItEasy;

    using Xunit;

    public class SearchServiceTest
    {
        private readonly IImageDataRepository _repository;

        public SearchServiceTest()
        {
            _repository = A.Fake<IImageDataRepository>();
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
            var src = A.Dummy<ImageData>();
            A.CallTo(() => _repository.FindSimilar(src, 1, 2, 3, 4, 5)).Returns(new List<ImageData>());

            // act
            var result = sut.FindSimilar(src, 1, 2, 3, 4, 5);

            // assert
            A.CallTo(() => _repository.FindSimilar(src, 1, 2, 3, 4, 5)).MustHaveHappenedOnceExactly();
            Assert.Equal(new List<ImageData>(), result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetCallsRepositoryAndReturnsItResultTest(bool resultFound)
        {
            // arrange
            var sut = new SearchService(_repository);
            var id = A.Dummy<string>();
            var fileIndex = resultFound ? new ImageData("") : null;
            A.CallTo(() => _repository.Get(id)).Returns(fileIndex);

            // act
            var result = sut.FindById(id);

            // assert
            A.CallTo(() => _repository.Get(id)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.Equal(fileIndex, result);
        }

    }
}