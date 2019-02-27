namespace EagleEye.ImageHash.Test
{
    using System;

    using EagleEye.Core.Interfaces.Core;
    using FakeItEasy;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class ImageHashPluginTest
    {
        private readonly ImageHashPlugin sut;
        private readonly Container container;

        public ImageHashPluginTest()
        {
            sut = new ImageHashPlugin();
            container = new Container();
        }

        [Fact]
        public void Name_ShouldNotBeNullOrWhitespace()
        {
            // arrange

            // act
            var result = sut.Name;

            // assert
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void EnablePlugin_ShouldThrowOnEmptyContainer()
        {
            // arrange

            // act
            sut.EnablePlugin(container);

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void EnablePlugin_ShouldSucceedWhenDependenciesAreRegisteredInContainer()
        {
            // arrange
            container.Register(A.Dummy<IFileService>, Lifestyle.Singleton);

            // act
            sut.EnablePlugin(container);

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }
    }
}
