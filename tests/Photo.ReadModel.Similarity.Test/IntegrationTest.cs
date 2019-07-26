namespace Photo.ReadModel.Similarity.Test
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using CQRSlite.Routing;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.Similarity.Interface;
    using EagleEye.TestHelper;
    using FakeItEasy;
    using FakeItEasy.Sdk;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class IntegrationTest
    {
        private const string ExistingImageFilename = "1.jpg";
        private readonly Container container;
        private readonly IFileService fileService;

        public IntegrationTest()
        {
            container = new Container();
            RegisterExternalDependencies(container);
            fileService = A.Fake<IFileService>();

            EagleEye.Photo.ReadModel.Similarity.Bootstrapper.Bootstrap(
               container,
               "InMemory a",
               "InMemory b");

            DoCqrsLiteStuff(container);

            container.Verify();

            A.CallTo(() => fileService.OpenRead(ExistingImageFilename))
             .ReturnsLazily(call => EagleEye.TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename));

            TestImages.ReadRelativeImageFile(ExistingImageFilename).Should().NotBeNull("This testsuite relies on this.");
        }

        [Fact]
        public async Task EnablePlugin_And_ProvideHashesForImage()
        {
            var initializers = container.GetAllInstances<IEagleEyeInitialize>().ToArray();
            initializers.Should().HaveCount(1);
            var initializer = initializers.Single();
            await initializer.InitializeAsync();

            var processes = container.GetAllInstances<IEagleEyeProcess>().ToArray();
            processes.Should().HaveCount(1);
            var process = processes.Single();
            process.Start();

            var similarityReadModel = container.GetInstance<ISimilarityReadModel>();
            var hashAlgorihms = await similarityReadModel.GetHashAlgorithmsAsync();
            hashAlgorihms.Should().BeEmpty();

            var publisher = container.GetInstance<IEventPublisher>();
            var evt = new PhotoHashAdded(Guid.NewGuid(), "AverageHash", 13213);
            await publisher.Publish(evt, CancellationToken.None);

            hashAlgorihms = await similarityReadModel.GetHashAlgorithmsAsync();
            hashAlgorihms.Should().BeEquivalentTo("AverageHash");

            process.Stop();
            container.Dispose();
        }

        private static void RegisterExternalDependencies(Container container)
        {
            foreach (var @type in EagleEye.Photo.ReadModel.Similarity.Bootstrapper.ExternalRequiredInterfaces())
                container.Register(@type, () => Create.Dummy(@type));
        }

        private static void DoCqrsLiteStuff(Container container)
        {
            container.Register<Router>(Lifestyle.Singleton);
            container.Register<IEventPublisher>(container.GetInstance<Router>, Lifestyle.Singleton);
            container.Register<IHandlerRegistrar>(container.GetInstance<Router>, Lifestyle.Singleton);

            // make sure that the CqrsLite lib has knowledge how to create the event handlers.
            // in the feature, this should be different.
            var registrar = new RouteRegistrar(container);
            registrar.RegisterHandlers(EagleEye.Photo.ReadModel.Similarity.Bootstrapper.GetEventHandlerTypes());
        }

        private void RegisterPluginExternalDependencies(Container container)
        {
            container.Register(() => fileService, Lifestyle.Singleton);
        }
    }
}
