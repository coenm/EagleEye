namespace EagleEye.Photo.Domain.Test
{
    using System;

    using CQRSlite.Domain;
    using FakeItEasy;
    using FakeItEasy.Sdk;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    using Sut = EagleEye.Photo.Domain.Bootstrapper;

    public class BootstrapperTest
    {
        [Fact]
        public void Bootstrap_ShouldOnlyDependOnExternalISession()
        {
            // arrange
            var container = new Container();
            RegisterExternalDependencies(container);
            container.Register(A.Dummy<ISession>);

            // act
            Sut.BootstrapPhotoDomain(container);

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }

        private static void RegisterExternalDependencies(Container container)
        {
            foreach (var @type in Sut.ExternalRequiredInterfaces())
                container.Register(@type, () => Create.Dummy(@type));
        }
    }
}
