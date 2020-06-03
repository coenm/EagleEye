namespace Photo.ReadModel.Similarity.Test.Internal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Photo.ReadModel.Similarity.Internal;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class SimilarityReadModelTest
    {
        private readonly IInternalStatelessSimilarityRepository repository;
        private readonly ISimilarityDbContextFactory contextFactory;
        private readonly IDateTimeService dateTimeService;
        private readonly ISimilarityDbContext dbContext;
        private readonly SimilarityReadModel sut;
        private readonly Guid photoGuid = new Guid("37FD10EA-C25C-4812-8674-F53E0373C3E7");

        public SimilarityReadModelTest()
        {
            repository = A.Fake<IInternalStatelessSimilarityRepository>();
            contextFactory = A.Fake<ISimilarityDbContextFactory>();
            dateTimeService = A.Fake<IDateTimeService>();

            dbContext = A.Fake<ISimilarityDbContext>();
            A.CallTo(() => contextFactory.CreateDbContext()).Returns(dbContext);

            sut = new SimilarityReadModel(repository, contextFactory, dateTimeService);
        }

        [Fact]
        public void Ctor_ShouldSucceed_WhenArgumentsAreNotNull()
        {
            // arrange

            // act
            Action act = () => _ = new SimilarityReadModel(repository, contextFactory, dateTimeService);

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetHashAlgorithmsAsync_ShouldQueryAndReturnEmpty_WhenRepositoryResultIsNull()
        {
            // arrange
            SetupHashIdentifiers(null);

            // act
            var result = await sut.GetHashAlgorithmsAsync();

            // assert
            result.Should().BeEmpty();
            A.CallTo(() => dbContext.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetHashAlgorithmsAsync_ShouldQueryAndMap_WhenRepositoryResultIsNotNullOrEmpty()
        {
            // arrange
            SetupHashIdentifiers(
                                 CreateHashIdentifiers(1, "Hash 1"),
                                 CreateHashIdentifiers(3, "Hash 3"));

            // act
            var result = await sut.GetHashAlgorithmsAsync();

            // assert
            result.Should().BeEquivalentTo("Hash 1", "Hash 3");
            A.CallTo(() => dbContext.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "TestCase")]
        public async Task CountSimilaritiesAsync_ShouldReturnZero_WhenArgumentIsNull()
        {
            // arrange
            var hashAlgorithm = (string)null;

            // act
            var result = await sut.CountSimilaritiesAsync(photoGuid, hashAlgorithm, 95);

            // assert
            result.Should().Be(0);
            A.CallTo(contextFactory).MustNotHaveHappened();
        }

        [Fact]
        public async Task CountSimilaritiesAsync_ShouldReturnZero_WhenHashIdentifierNotFound()
        {
            // arrange
            SetupHashIdentifiers(
                                 CreateHashIdentifiers(1, "Hash 1"),
                                 CreateHashIdentifiers(3, "Hash 3"));

            // act
            var result = await sut.CountSimilaritiesAsync(photoGuid, "Hash 2", 95);

            // assert
            result.Should().Be(0);
            A.CallTo(repository).MustHaveHappenedOnceExactly();
            A.CallTo(() => dbContext.Dispose()).MustHaveHappenedOnceExactly();
        }

        private static HashIdentifiers CreateHashIdentifiers(int id, string name)
        {
            return new HashIdentifiers
                   {
                       Id = id,
                       HashIdentifier = name,
                   };
        }

        private void SetupHashIdentifiers(params HashIdentifiers[] hashIdentifiers)
        {
            A.CallTo(() => repository.GetAllHashIdentifiersAsync(dbContext)).Returns(Task.FromResult(hashIdentifiers));
        }
    }
}
