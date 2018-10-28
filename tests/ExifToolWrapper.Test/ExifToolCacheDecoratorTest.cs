namespace EagleEye.ExifToolWrapper.Test
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;

    using FakeItEasy;

    using FluentAssertions;

    using Newtonsoft.Json.Linq;

    using Xunit;

    public class ExifToolCacheDecoratorTest : IDisposable
    {
        private const string Filename1 = "FILENAME1.jpg";
        private const string Filename2 = "FILENAME2.jpg";
        private const string Filename3 = "FILENAME3.jpg";
        private readonly JObject fileResult1;
        private readonly JObject fileResult2;
        private readonly JObject fileResult3;
        private readonly ExifToolCacheDecorator sut;
        private readonly IExifTool decoratee;
        private readonly IDateTimeService dateTimeService;
        private readonly DateTime dtInit;

        public ExifToolCacheDecoratorTest()
        {
            decoratee = A.Fake<IExifTool>();

            fileResult1 = new JObject();
            fileResult2 = new JObject();
            fileResult3 = new JObject();

            dtInit = new DateTime(2000, 1, 2, 3, 4, 5);

            A.CallTo(() => decoratee.GetMetadataAsync(Filename1)).Returns(Task.FromResult(fileResult1));
            A.CallTo(() => decoratee.GetMetadataAsync(Filename2)).Returns(Task.FromResult(fileResult2));
            A.CallTo(() => decoratee.GetMetadataAsync(Filename3)).Returns(Task.FromResult(fileResult3));

            dateTimeService = A.Fake<IDateTimeService>();


            sut = new ExifToolCacheDecorator(decoratee, dateTimeService);
        }

        public void Dispose()
        {
            sut?.Dispose();
        }

        [Fact]
        public void Dispose_ShouldCallDecorateeDisposeTest()
        {
            // arrange

            // act
            sut.Dispose();

            // assert
            A.CallTo(() => decoratee.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMetadataAsync_ShouldGetAndReturnMetadataFromDecorateeTest()
        {
            // arrange
            A.CallTo(() => dateTimeService.Now).Returns(dtInit);

            // act
            var result = await sut.GetMetadataAsync(Filename1).ConfigureAwait(false);

            // assert
            result.Should().BeSameAs(fileResult1);
            A.CallTo(() => decoratee.GetMetadataAsync(Filename1)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMetadataAsyncTwiceWithinCacheTimeoutShouldGetAndReturnMetadataFromDecorateeTest()
        {
            // arrange
            A.CallTo(() => dateTimeService.Now)
             .ReturnsNextFromSequence(dtInit, dtInit.AddMinutes(2));

            // act
            var result1Task = sut.GetMetadataAsync(Filename1);
            var result2Task = sut.GetMetadataAsync(Filename1);

            var result1 = await result1Task.ConfigureAwait(false);
            var result2 = await result2Task.ConfigureAwait(false);

            // assert
            result1.Should().BeSameAs(fileResult1);
            result2.Should().BeSameAs(fileResult1);
            A.CallTo(() => decoratee.GetMetadataAsync(Filename1)).MustHaveHappenedOnceExactly();
        }
    }
}