namespace EagleEye.ImageHash.Test
{
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.TestHelper;
    using FakeItEasy;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class IntegrationTest
    {
        private const string ExistingImageFilename = "1.jpg";
        private readonly ImageHashPackage package;
        private readonly Container packageContainer;
        private readonly Container container;
        private readonly IFileService fileService;

        public IntegrationTest()
        {
            package = new ImageHashPackage();
            packageContainer = new Container();
            container = new Container();
            fileService = A.Fake<IFileService>();

            A.CallTo(() => fileService.OpenRead(ExistingImageFilename))
             .ReturnsLazily(call => TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename));

            TestImages.ReadRelativeImageFile(ExistingImageFilename).Should().NotBeNull("This testsuite relies on this.");
        }

        [Fact]
        public async Task EnablePlugin_And_ProvideHashesForImage()
        {
            package.RegisterServices(packageContainer);
            var plugins = packageContainer.GetAllInstances<IEagleEyePlugin>().ToArray();
            plugins.Should().ContainSingle();
            var singlePlugin = plugins.Single();

            RegisterPluginExternalDependencies(container);
            singlePlugin.EnablePlugin(container);
            container.Verify();

            var photoHashProviders = container.GetAllInstances<IPhotoHashProvider>().ToArray();
            photoHashProviders.Should().ContainSingle();
            var photoHashProvider = photoHashProviders.Single();

            const string filename = "dummy_not_existing_image.jpg";
            photoHashProvider.CanProvideInformation(filename).Should().BeTrue();
            var photoHashes = await photoHashProvider.ProvideAsync(filename, null);
            photoHashes.Should().BeNullOrEmpty();

            // existing file with image stream
            photoHashProvider.CanProvideInformation(ExistingImageFilename).Should().BeTrue();
            photoHashes = await photoHashProvider.ProvideAsync(ExistingImageFilename, null);
            photoHashes.Should().BeEquivalentTo(
                new object[]
                {
                    CreatePhotoHash("AverageHash", 18442214084176449028),
                    CreatePhotoHash("DifferenceHash", 3573764330010097788),
                    CreatePhotoHash("PerceptualHash", 15585629762494286247),
                });

            container.Dispose();
        }

        private static PhotoHash CreatePhotoHash(string name, ulong value)
        {
            return new PhotoHash
                   {
                       Hash = value,
                       HashName = name,
                   };
        }

        private void RegisterPluginExternalDependencies(Container container)
        {
            container.Register(() => fileService, Lifestyle.Singleton);
        }
    }
}
