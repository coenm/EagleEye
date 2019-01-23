namespace Photo.ReadModel.Similarity.Test
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Photo.ReadModel.Similarity.Bootstrapper;

    public class BootstrapperTest
    {
        [Fact]
        public void Bootstrap_ShouldBeValid_WhenGivenTwoInMemoryDatabases()
        {
            // arrange
            var container = new Container();

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
            Sut.Bootstrap(container, "InMemory a", "InMemory b");

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
