namespace EagleEye.DirectoryStructure.Test
{
    using System;

    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class DirectoryStructurePluginTest
    {
        private readonly DirectoryStructurePlugin sut;
        private readonly Container container;

        public DirectoryStructurePluginTest()
        {
            sut = new DirectoryStructurePlugin();
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
