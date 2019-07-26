namespace Photo.ReadModel.Similarity.Test
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Module;
    using FakeItEasy.Sdk;
    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Photo.ReadModel.Similarity.Bootstrapper;

    public class BootstrapperTest
    {
        [Fact]
        public void Bootstrap_ShouldNotBeValid_WhenContainerHasNoRegistrationsForExternalDependencies()
        {
            // arrange
            var container = new Container();

            // assume
            Sut.ExternalRequiredInterfaces().Should().NotBeEmpty("otherwise the validation of the container will not fail ;-)");

            // act
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Bootstrap_ShouldBeValid_WhenGivenTwoInMemoryDatabasesAndRegisterExternalDependencies()
        {
            // arrange
            var container = new Container();
            RegisterExternalDependencies(container);

            // act
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }

        [Fact]
        public void Bootstrap_ShouldRegisterAtLeastOneIEagleEyeInitialize()
        {
            // arrange
            var container = new Container();
            RegisterExternalDependencies(container);

            // act
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

            // assert
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>();
            instancesToInitialize.Should().NotBeEmpty();
        }

        [Fact]
        public void Bootstrap_ShouldAppendAndNotOverrideEagleEyeInitializersToCollection()
        {
            // arrange
            var container = new Container();
            RegisterExternalDependencies(container);
            container.Collection.Append<IEagleEyeInitialize, FakeEagleEyeInitialize>();

            // act
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

            // assert
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();
            instancesToInitialize.Should().Contain(instance => instance is FakeEagleEyeInitialize);
        }

        [Fact]
        public void InitializeAsync_ShouldNotThrowException()
        {
            // arrange
            var container = new Container();
            RegisterExternalDependencies(container);
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

            // act
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();

            // assert
            Func<Task> assert = async () => await Task.WhenAll(instancesToInitialize.Select(instance => instance.InitializeAsync()));
            assert.Should().NotThrow();
        }

        private static void RegisterExternalDependencies(Container container)
        {
            foreach (var @type in Sut.ExternalRequiredInterfaces())
                container.Register(@type, () => Create.Dummy(@type));
        }

        [UsedImplicitly]
        private class FakeEagleEyeInitialize : IEagleEyeInitialize
        {
            public Task InitializeAsync() => Task.CompletedTask;
        }
    }
}
