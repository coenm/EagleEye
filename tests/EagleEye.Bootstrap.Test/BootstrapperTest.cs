namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using EagleEye.Core.Interfaces.Module;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.Test;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Bootstrap.Bootstrapper;

    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Reviewed.")]
    public class BootstrapperTest
    {
        private readonly string tempPath;
        private readonly IEnumerable<IEagleEyePlugin> plugins;
        private readonly Dictionary<string, object> config;

        public BootstrapperTest()
        {
            tempPath = Path.GetTempPath();
            plugins = Sut.FindAvailablePlugins();
            config = new Dictionary<string, object>
                {
                    // { ExifToolPlugin.ConfigKeyExiftoolPluginFullConfigFile, "" },
                    { ExifToolPlugin.ConfigKeyExiftoolPluginFullExe, ExifToolSystemConfiguration.ExifToolExecutable },
                };
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
            var sut = Sut.Initialize(plugins, config);
            using var container = sut.Finalize();

            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Bootstrap_ShouldNotThrow_WhenRegisterPhotoDatabaseReadModel()
        {
            // arrange

            // act
            var sut = Sut.Initialize(plugins, config);
            sut.RegisterPhotoDatabaseReadModel("InMemory a");
            using var container = sut.Finalize();

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
            var sut = Sut.Initialize(plugins, config);
            sut.RegisterSearchEngineReadModel(searchEngineDirectory);
            using var container = sut.Finalize();

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
            var sut = Sut.Initialize(plugins, config);
            sut.RegisterSimilarityReadModel(connectionString, connectionStringHangFire);
            using var container = sut.Finalize();

            Action act = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // assert
            act.Should().NotThrow();
        }
    }
}
