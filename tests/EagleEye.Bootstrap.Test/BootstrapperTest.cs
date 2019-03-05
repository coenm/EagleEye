namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Bootstrap.Bootstrapper;

    public class BootstrapperTest
    {
        [NotNull] private static readonly Container Container = new Container();

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
            var plugins = Sut.FindAvailablePlugins();

            // act
            Sut.Bootstrap(Container, "dummy base directory", plugins);
            Action act = () => Container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }
    }
}
