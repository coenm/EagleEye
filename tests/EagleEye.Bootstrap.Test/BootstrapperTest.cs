namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using EagleEye.Core.Interfaces.Module;
    using EagleEye.ExifTool;
    using FakeItEasy;
    using FluentAssertions;
    using Helpers.Guards;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Bootstrap.Bootstrapper;

    public class BootstrapperTest
    {
        private readonly string tempPath;
        private readonly IEnumerable<IEagleEyePlugin> plugins;

        public BootstrapperTest()
        {
            tempPath = Path.GetTempPath();
            plugins = Sut.FindAvailablePlugins();
        }

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

            // act
            var sut = Sut.Initialize(tempPath, plugins);
            sut.AddRegistrations(MakeSureExifToolCanBeFound);
            var container = sut.Finalize();

            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Bootstrap_ShouldNotThrow_WhenRegisterPhotoDatabaseReadModel()
        {
            // arrange

            // act
            var sut = Sut.Initialize(tempPath, plugins);
            sut.RegisterPhotoDatabaseReadModel("InMemory a");
            sut.AddRegistrations(MakeSureExifToolCanBeFound);
            var container = sut.Finalize();

            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Bootstrap_ShouldNotThrow_WhenRegisterSearchEngineReadModel()
        {
            // arrange
            var searchEngineDirectory = Path.Combine(tempPath, "Lucene");

            // act
            var sut = Sut.Initialize(tempPath, plugins);
            sut.RegisterSearchEngineReadModel(searchEngineDirectory);
            sut.AddRegistrations(MakeSureExifToolCanBeFound);
            var container = sut.Finalize();

            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Bootstrap_ShouldNotThrow_WhenRegisterSimilarityReadModel()
        {
            // arrange
            const string connectionString = "InMemory a";
            const string connectionStringHangFire = "InMemory b";

            // act
            var sut = Sut.Initialize(tempPath, plugins);
            sut.RegisterSimilarityReadModel(connectionString, connectionStringHangFire);
            sut.AddRegistrations(MakeSureExifToolCanBeFound);
            var container = sut.Finalize();

            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }

        private void MakeSureExifToolCanBeFound(Container container)
        {
            DebugGuard.NotNull(container, nameof(container));

            container.Options.AllowOverridingRegistrations = true;
            container.Register<IExifTool>(A.Dummy<IExifTool>, Lifestyle.Singleton);
        }
    }
}
