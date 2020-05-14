namespace Photo.ReadModel.SearchEngineLucene.Test
{
    using System;

    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Photo.ReadModel.SearchEngineLucene.Bootstrapper;

    public class BootstrapperTest
    {
        [Fact]
        public void Bootstrap_ShouldBeValid_WhenUsingTheInMemoryDatabase()
        {
            // arrange
            var container = new Container();

            // act
            Sut.BootstrapSearchEngineLuceneReadModel(container);

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }
    }
}
