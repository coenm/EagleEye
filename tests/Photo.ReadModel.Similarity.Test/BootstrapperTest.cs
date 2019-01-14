namespace Photo.ReadModel.Similarity.Test
{
    using System;

    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    using Sut = Photo.ReadModel.Similarity.Bootstrapper;

    public class BootstrapperTest
    {
        [Fact]
        public void Bootstrap_ShouldBeValid_WhenGivenTwoInMemoryDatabases()
        {
            // arrange
            var container = new Container();

            // act
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }
    }
}
