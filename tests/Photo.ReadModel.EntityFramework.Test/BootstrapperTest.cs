namespace Photo.ReadModel.EntityFramework.Test
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Module;
    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Photo.ReadModel.EntityFramework.Bootstrapper;

    public class BootstrapperTest
    {
        [Fact]
        public void Bootstrap_ShouldBeValid_WhenGivenInMemoryDatabase()
        {
            // arrange
            var container = new Container();

            // act
            Sut.BootstrapEntityFrameworkReadModel(container, "InMemory a");

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }

        [Fact]
        public void Bootstrap_ShouldRegisterAtLeastOneIEagleEyeInitialize()
        {
            // arrange
            var container = new Container();

            // act
            Sut.BootstrapEntityFrameworkReadModel(container, "InMemory a");

            // assert
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>();
            instancesToInitialize.Should().NotBeEmpty();
        }

        [Fact]
        public void Bootstrap_ShouldAppendAndNotOverrideEagleEyeInitializersToCollection()
        {
            // arrange
            var container = new Container();
            container.Collection.Append<IEagleEyeInitialize, FakeEagleEyeInitialize>();

            // act
            Sut.BootstrapEntityFrameworkReadModel(container, "InMemory a");

            // assert
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();
            instancesToInitialize.Should().Contain(instance => instance is FakeEagleEyeInitialize);
        }

        [Fact]
        public void InitializeAsync_ShouldNotThrowException()
        {
            // arrange
            var container = new Container();
            Sut.BootstrapEntityFrameworkReadModel(container, "InMemory a");

            // act
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();

            // assert
            Func<Task> assert = async () => await Task.WhenAll(instancesToInitialize.Select(instance => instance.InitializeAsync()));
            assert.Should().NotThrow();
        }

        [UsedImplicitly]
        private class FakeEagleEyeInitialize : IEagleEyeInitialize
        {
            public Task InitializeAsync() => Task.CompletedTask;
        }
    }
}
