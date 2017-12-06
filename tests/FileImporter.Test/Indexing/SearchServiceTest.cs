using System;
using System.Linq;
using FakeItEasy;
using FileImporter.Indexing;
using Xunit;

namespace FileImporter.Test.Indexing
{
    public class SearchServiceTest
    {
        private readonly IFileIndexRepository _fileIndexRepository;

        public SearchServiceTest()
        {
            _fileIndexRepository = A.Fake<IFileIndexRepository>();
        }

        /// <summary>
        /// Use real dataset with real values. the 'wa' images should be found based on the original.
        /// </summary>
        [Theory]
        [InlineData("1.jpg", "1wa.jpg")]
        [InlineData("2.jpg", "2wa.jpg")]
        [InlineData("3.jpg", "3wa.jpg")]
        [InlineData("4.jpg", "4wa.jpg")]
        [InlineData("6.jpg", "6wa.jpg")]
        [InlineData("7.jpg", "7wa.jpg")]
        [InlineData("8.jpg", null)]
        public void FindSimilarImageTest(string identifier, string expectedMatch)
        {
            // arrange
            A.CallTo(() => _fileIndexRepository.Find(A<Predicate<FileIndex>>._, A<int>._, A<int>._))
                .ReturnsLazily((Predicate<FileIndex> predicate, int _, int __) =>
                {
                    return TestImagesIndex.Index.Where(index => predicate(index));
                });
            var src = TestImagesIndex.Index.Single(index => index.Identifier == identifier);
            var sut = new SearchService(_fileIndexRepository);

            // act
            var result = sut.FindSimilar(src).ToList();

            // assert
            if (expectedMatch == null)
                Assert.Empty(result);
            else
                Assert.Single(result, TestImagesIndex.Index.Single(index => index.Identifier == expectedMatch));

        }
    }
}