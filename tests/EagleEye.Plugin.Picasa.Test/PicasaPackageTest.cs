namespace EagleEye.Picasa.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Picasa;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class PicasaPackageTest
    {
        private readonly PicasaPackage sut;
        private readonly Container container;

        public PicasaPackageTest()
        {
            sut = new PicasaPackage();
            container = new Container();
        }

        [Fact]
        public void RegisterServices_ShouldRegisterDirectoryStructurePlugin_WhenContainerIsNotNull()
        {
            // arrange

            // act
            sut.RegisterServices(container);
            var plugins = container.GetAllInstances<IEagleEyePlugin>().ToArray();

            // assert
            plugins.Should().ContainSingle().Which.Should().BeOfType<PicasaPlugin>();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Test case")]
        public void RegisterServices_ShouldNotThrow_WhenContainerIsNull()
        {
            // arrange

            // act
            Action act = () => sut.RegisterServices(null);

            // assert
            act.Should().NotThrow();
        }
    }
}
