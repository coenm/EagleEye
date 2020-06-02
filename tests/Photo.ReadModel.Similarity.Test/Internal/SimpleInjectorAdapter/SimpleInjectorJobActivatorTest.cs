namespace Photo.ReadModel.Similarity.Test.Internal.SimpleInjectorAdapter
{
    using EagleEye.Photo.ReadModel.Similarity.Internal.SimpleInjectorAdapter;
    using FluentAssertions;
    using SimpleInjector;
    using Xunit;

    public class SimpleInjectorJobActivatorTest
    {
        [Fact]
        public void ActivateJob_ShouldReturnInstanceOfRequestedType_WhenContainerIsAbleToConstruct()
        {
            // arrange
            var container = new Container();
            var sut = new SimpleInjectorJobActivator(container);

            // act
            var jobType = typeof(TestJob);
            var result = sut.ActivateJob(jobType);

            // assert
            result.Should().BeOfType<TestJob>();
        }

        private class TestJob
        {
        }
    }
}
