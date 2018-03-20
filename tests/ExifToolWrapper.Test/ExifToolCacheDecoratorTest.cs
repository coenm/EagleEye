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
        private const string FILENAME_1 = "FILENAME1.jpg";
        private const string FILENAME_2 = "FILENAME2.jpg";
        private const string FILENAME_3 = "FILENAME3.jpg";
        private readonly JObject _fileResult1;
        private readonly JObject _fileResult2;
        private readonly JObject _fileResult3;
        private readonly ExifToolCacheDecorator _sut;
        private readonly IExifTool _decoratee;
        private readonly IDateTimeService _dateTimeService;
        private readonly DateTime _dtInit;

        public ExifToolCacheDecoratorTest()
        {
            _decoratee = A.Fake<IExifTool>();

            _fileResult1 = new JObject();
            _fileResult2 = new JObject();
            _fileResult3 = new JObject();

            _dtInit = new DateTime(2000, 1, 2, 3, 4, 5);

            A.CallTo(() => _decoratee.GetMetadataAsync(FILENAME_1)).Returns(Task.FromResult(_fileResult1));
            A.CallTo(() => _decoratee.GetMetadataAsync(FILENAME_2)).Returns(Task.FromResult(_fileResult2));
            A.CallTo(() => _decoratee.GetMetadataAsync(FILENAME_3)).Returns(Task.FromResult(_fileResult3));

            _dateTimeService = A.Fake<IDateTimeService>();


            _sut = new ExifToolCacheDecorator(_decoratee, _dateTimeService);
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }

        [Fact]
        public void Dispose_ShouldCallDecorateeDisposeTest()
        {
            // arrange

            // act
            _sut.Dispose();

            // assert
            A.CallTo(() => _decoratee.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMetadataAsync_ShouldGetAndReturnMetadataFromDecorateeTest()
        {
            // arrange
            A.CallTo(() => _dateTimeService.Now).Returns(_dtInit);

            // act
            var result = await _sut.GetMetadataAsync(FILENAME_1).ConfigureAwait(false);

            // assert
            result.Should().BeSameAs(_fileResult1);
            A.CallTo(() => _decoratee.GetMetadataAsync(FILENAME_1)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMetadataAsyncTwiceWithinCacheTimeoutShouldGetAndReturnMetadataFromDecorateeTest()
        {
            // arrange
            A.CallTo(() => _dateTimeService.Now)
             .ReturnsNextFromSequence(_dtInit, _dtInit.AddMinutes(2));

            // act
            var result1Task = _sut.GetMetadataAsync(FILENAME_1);
            var result2Task = _sut.GetMetadataAsync(FILENAME_1);

            var result1 = await result1Task.ConfigureAwait(false);
            var result2 = await result2Task.ConfigureAwait(false);

            // assert
            result1.Should().BeSameAs(_fileResult1);
            result2.Should().BeSameAs(_fileResult1);
            A.CallTo(() => _decoratee.GetMetadataAsync(FILENAME_1)).MustHaveHappenedOnceExactly();
        }
    }
}