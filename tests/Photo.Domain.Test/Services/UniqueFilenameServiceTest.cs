namespace EagleEye.Photo.Domain.Test.Services
{
    using System.Diagnostics.CodeAnalysis;

    using EagleEye.Photo.Domain.Services;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class UniqueFilenameServiceTest
    {
        private readonly UniqueFilenameService sut;
        private readonly IFilenameRepository repository;
        private readonly string filename;

        public UniqueFilenameServiceTest()
        {
            filename = "dummy";
            repository = A.Fake<IFilenameRepository>();
            sut = new UniqueFilenameService(repository);
        }

        [Fact]
        public void Claim_ShouldReturnNull_WhenFileExists()
        {
            // arrange
            A.CallTo(() => repository.Contains(filename)).Returns(true);

            // act
            var result = sut.Claim(filename);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public void Claim_ShouldCreateClaimFilenameToken_WhenFileDoesNotExists()
        {
            // arrange
            A.CallTo(() => repository.Contains(filename)).Returns(false);

            // act
            var result = sut.Claim(filename);

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Is not null")]
        public void DisposingClaim_ShouldNotSaveFilenameToRepository()
        {
            // arrange
            A.CallTo(() => repository.Contains(filename)).Returns(false);
            var claim = sut.Claim(filename);

            // act
            claim.Dispose();

            // assert
            A.CallTo(() => repository.Add(filename)).MustNotHaveHappened();
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Is not null")]
        public void CommitClaim_ShouldSaveFilenameToRepository()
        {
            // arrange
            A.CallTo(() => repository.Contains(filename)).Returns(false);
            using (var claim = sut.Claim(filename))
            {
                // act
                claim.Commit();

                // assert
                A.CallTo(() => repository.Add(filename)).MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Is not null")]
        public void Claim_ShouldPreventSecondClaim()
        {
            // arrange
            A.CallTo(() => repository.Contains(filename)).Returns(false);
            _ = sut.Claim(filename);

            // act
            var result = sut.Claim(filename);

            // assert
            result.Should().BeNull();
        }
    }
}
