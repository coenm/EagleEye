namespace EagleEye.DirectoryStructure.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using EagleEye.Core.Interfaces.Module;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class PackageIntegrationTest
    {
        private readonly Container container;

        public PackageIntegrationTest()
        {
            container = new Container();
        }

        [Fact]
        public void RegisterPackages_ShouldRegisterEagleEyePluginDirectoryStructure()
        {
            // arrange
            var assemblies = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory))
                .GetFiles()
                .Where(file =>
                    file.Name.StartsWith("EagleEye.Plugin.DirectoryStructure.dll")
                    &&
                    file.Extension.ToLower() == ".dll")
                .Select(file => Assembly.Load(AssemblyName.GetAssemblyName(file.FullName)))
                .ToArray();

            // act
            container.RegisterPackages(assemblies);
            var plugins = container.GetAllInstances<IEagleEyePlugin>().ToArray();

            // assert
            assemblies.Should().ContainSingle();
            plugins.Should().ContainSingle().Which.Should().BeOfType<DirectoryStructurePlugin>();
        }
    }
}
