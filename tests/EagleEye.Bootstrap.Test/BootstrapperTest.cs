namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.IO;
    using System.Linq;

    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Bootstrap.Bootstrapper;

    public class BootstrapperTest
    {
        /// <summary>
        /// This test depends on the dependency projects.
        /// </summary>
        [Fact]
        public void FindAvailablePlugins_ShouldReturnListOfFoundPlugins()
        {
            // arrange

            // act
            var result = Sut.FindAvailablePlugins().ToList();

            // assert
            result.Select(x => x.Name).Should().BeEquivalentTo("DirectoryStructurePlugin", "ExifToolPlugin", "ImageHashPlugin", "PicasaPlugin");
        }

        [Fact]
        public void Bootstrap_ShouldNotThrow()
        {
            // arrange
            var tempPath = Path.GetTempPath();
            var plugins = Sut.FindAvailablePlugins();

            // act
            var container = Sut.Initialize(tempPath, plugins).Finalize();
            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }
    }
}
