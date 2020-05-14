namespace EagleEye.FileImporter.Test
{
    using System;

    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class StartupTest
    {
        [Fact]
        public void ConfigureContainer_ShouldSucceed_WhenConnectionStringsAreFilled()
        {
            // arrange
            var container = new Container();
            var connectionStrings = new ConnectionStrings
                {
                    FilenameEventStore = "dummy",
                    HangFire = "InMemory HangFire",
                    Similarity = "InMemory Similarity",
                    ConnectionStringPhotoDatabase = "InMemory Photos",
                    LuceneDirectory = ConnectionStrings.LuceneInMemory,
                };

            // act
            Action act = () =>
                         {
                             Startup.ConfigureContainer(connectionStrings);
                             Startup.VerifyContainer(container);
                         };

            // assert
            act.Should().NotThrow();
        }
    }
}
