namespace Photo.ReadModel.Similarity.Test.Internal.Processing
{
    using System;

    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.MemoryStorage;
    using Xunit;

    public class HangFireServerEagleEyeProcessTest
    {
        [Fact]
        public void Start_ShouldNotThrow()
        {
            // arrange
            JobStorage.Current = new MemoryStorage(new MemoryStorageOptions());
            var sut = new HangFireServerEagleEyeProcess();

            // act
            Action act = () => sut.Start();

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void StartDouble_ShouldNotThrow_WhenNotStoppedInBetween()
        {
            // arrange
            JobStorage.Current = new MemoryStorage(new MemoryStorageOptions());
            var sut = new HangFireServerEagleEyeProcess();

            // act
            Action act = () =>
                {
                    sut.Start();
                    sut.Start();
                };

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Stop_ShouldNotThrow_WhenNotStarted()
        {
            // arrange
            var sut = new HangFireServerEagleEyeProcess();

            // act
            Action act = () => sut.Stop();

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void MultipleStops_ShouldNotThrow_WhenNotStarted()
        {
            // arrange
            var sut = new HangFireServerEagleEyeProcess();

            // act
            Action act = () =>
                {
                    sut.Stop();
                    sut.Stop();
                    sut.Stop();
                };

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_ShouldNotThrow_WhenNotStartedNorStopped()
        {
            // arrange
            var sut = new HangFireServerEagleEyeProcess();

            // act
            Action act = () => sut.Dispose();

            // assert
            act.Should().NotThrow();
        }
    }
}
