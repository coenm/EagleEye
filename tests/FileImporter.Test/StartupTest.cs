namespace EagleEye.FileImporter.Test
{
    using System;

    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class StartupTest
    {
        [Fact]
        public void ConfigureContainer_ShouldSucceed()
        {
            // arrange
            var container = new Container();

            // act
            Action act = () =>
                         {
                             Startup.ConfigureContainer(container, "dummy", "InMemory hangfire");
                             Startup.VerifyContainer(container);
                         };

            // assert
            act.Should().NotThrow();
        }
    }
}
