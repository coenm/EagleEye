namespace EagleEye.ImageHash.Test
{
    using System;

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
        public void EnablePlugin()
        {
            // arrange

            // act
            sut.EnablePlugin(container);

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }
    }
}
