namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.IO;

    using FluentAssertions;
    using Xunit;

    using Sut = EagleEye.Bootstrap.PluginLocator;

    public class PluginLocatorTest
    {
        [Fact]
        public void FindPluginAssemblies_ShouldReturnEmptyList_WhenDirectoryDoesNotContainPluginAssemblies()
        {
            // arrange
            // assume that this directory doesn't have any plugin assemblies.
            var directory = Path.GetTempPath();

            // act
            var result = Sut.FindPluginAssemblies(directory);

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
