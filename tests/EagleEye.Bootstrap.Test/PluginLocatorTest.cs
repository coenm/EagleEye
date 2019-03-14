namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.IO;

    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    using Sut = EagleEye.Bootstrap.PluginLocator;

    public class PluginLocatorTest : IDisposable
    {
        private readonly ITestOutputHelper output;

        private readonly string tmpDirectory;

        public PluginLocatorTest(ITestOutputHelper output)
        {
            tmpDirectory = Path.GetTempPath();
            tmpDirectory = Path.Combine(tmpDirectory, DateTime.Now.Ticks.ToString() + new Random().Next(int.MaxValue));
            Directory.CreateDirectory(tmpDirectory);

            this.output = output;
        }

        public void Dispose()
        {
            Directory.Delete(tmpDirectory);
        }

        [Fact]
        public void FindPluginAssemblies_ShouldReturnEmptyList_WhenDirectoryDoesNotContainPluginAssemblies()
        {
            // arrange
            // assume that this directory doesn't have any plugin assemblies.
            output.WriteLine($"Directory : {tmpDirectory}");

            // act
            var result = Sut.FindPluginAssemblies(tmpDirectory);

            // assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void FindPluginAssemblies_ShouldReturnFoundPlugins_WhenDirectoryDoesContainsPluginAssemblies()
        {
            // arrange
            // assume that this directory has plugin assemblies
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

            // act
            var result = Sut.FindPluginAssemblies(directory);

            // assert
            result.Should().NotBeEmpty();
        }
    }
}
