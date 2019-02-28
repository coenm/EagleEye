namespace EagleEye.Core.Test.Bootstrapper
{
    using System.Linq;

    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Core.Bootstrapper.Bootstrapper;

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
        public void Bootstrap_ShouldNotThrow_WIP()
        {
            // arrange

            // act
            var plugins = Sut.FindAvailablePlugins();
            Sut.Bootstrap(Container, plugins);

            // assert
            Container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}
