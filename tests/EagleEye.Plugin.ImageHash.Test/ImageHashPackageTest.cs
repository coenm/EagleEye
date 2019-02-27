namespace EagleEye.ImageHash.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using EagleEye.Core.Interfaces.Module;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class ImageHashPackageTest
    {
        private readonly ImageHashPackage sut;
        private readonly Container container;

        public ImageHashPackageTest()
        {
            sut = new ImageHashPackage();
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
            plugins.Should().ContainSingle().Which.Should().BeOfType<ImageHashPlugin>();
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
