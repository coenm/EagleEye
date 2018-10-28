namespace EagleEye.FileImporter.Test.Infrastructure.FileIndexRepository
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;

    using FakeItEasy;

    using Xunit;

    public class SingleFileIndexRepositoryTest
    {
        private readonly SingleImageDataRepository sut;
        private readonly List<ImageData> fileIndex;
        private readonly IPersistentSerializer<List<ImageData>> storage;

        public SingleFileIndexRepositoryTest()
        {
            fileIndex = TestImagesIndex.Index;
            storage = A.Fake<IPersistentSerializer<List<ImageData>>>();
            A.CallTo(() => storage.Load()).Returns(fileIndex);

            sut = new SingleImageDataRepository(storage);
        }

        /// <summary>
        /// Use real dataset with real values. The 'wa' images should be found based on the original.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
            var src = fileIndex.Single(index => index.Identifier == identifier);

            // act
            var result = sut.FindSimilar(src).ToList();

            // assert
            A.CallTo(() => storage.Load()).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Save(A<List<ImageData>>._)).MustNotHaveHappened();
            if (expectedMatch == null)
                Assert.Empty(result);
            else
                Assert.Single(result, TestImagesIndex.Index.Single(index => index.Identifier == expectedMatch));
        }

        [Theory]
        [InlineData("1.jpg", 1)]
        [InlineData("2.jpg", 1)]
        [InlineData("3.jpg", 1)]
        [InlineData("4.jpg", 1)]
        [InlineData("6.jpg", 1)]
        [InlineData("7.jpg", 1)]
        [InlineData("8.jpg", 0)]
        public void CountSimilarTest(string identifier, int expectedCount)
        {
            // arrange
            var src = fileIndex.Single(index => index.Identifier == identifier);

            // act
            var result = sut.CountSimilar(src);

            // assert
            A.CallTo(() => storage.Load()).MustHaveHappenedOnceExactly();
            A.CallTo(() => storage.Save(A<List<ImageData>>._)).MustNotHaveHappened();
            Assert.Equal(expectedCount, result);
        }
    }
}