namespace EagleEye.Photo.Domain.Test
{
    using System;

    using CQRSlite.Domain;
    using FakeItEasy;
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
            container.Register(A.Dummy<ISession>);

            // act
            Sut.BootstrapPhotoDomain(container);

            // assert
            Action assert = () => container.Verify(VerificationOption.VerifyAndDiagnose);
            assert.Should().NotThrow();
        }
    }
}
