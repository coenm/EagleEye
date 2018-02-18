namespace EagleEye.ExifToolWrapper.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.TestImages;

    using FluentAssertions;

    using Xunit;

    public class ExifToolAdapterTest : IDisposable
    {
        private readonly string _imageFilename;
        private readonly ExifToolAdapter _sut;

        public ExifToolAdapterTest()
        {
            _imageFilename = Directory
                     .GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            _imageFilename.Should().NotBeNullOrEmpty();

            _sut = new ExifToolAdapter();
        }

        [Fact]
        public async Task GetMetadataAsyncWithPreparedImageShouldResultInDynamicObjectContainingExifSectionTest()
        {
            // arrange
            const string EXPECTED_EXIF = "{\r\n  \"ImageDescription\": \"happy puppy!\",\r\n  \"XResolution\": 72,\r\n  \"YResolution\": 72,\r\n  \"ResolutionUnit\": \"inches\",\r\n  \"Software\": \"Picasa\",\r\n  \"ModifyDate\": \"2018:02:07 22:27:46\",\r\n  \"YCbCrPositioning\": \"Centered\",\r\n  \"ExifVersion\": \"0231\",\r\n  \"DateTimeOriginal\": \"2034:02:01 12:21:12\",\r\n  \"CreateDate\": \"2034:02:01 12:21:12\",\r\n  \"ComponentsConfiguration\": \"Y, Cb, Cr, -\",\r\n  \"FlashpixVersion\": \"0100\",\r\n  \"ColorSpace\": \"Uncalibrated\",\r\n  \"ImageUniqueID\": \"32da8d4383d8922abdda96abd924a4d6\",\r\n  \"GPSVersionID\": \"2.2.0.0\",\r\n  \"GPSLatitudeRef\": \"North\",\r\n  \"GPSLatitude\": 40.736072,\r\n  \"GPSLongitudeRef\": \"West\",\r\n  \"GPSLongitude\": 73.994293,\r\n  \"GPSAltitudeRef\": \"Above Sea Level\",\r\n  \"GPSAltitude\": \"34 m\",\r\n  \"GPSTimeStamp\": \"17:21:12\",\r\n  \"GPSMapDatum\": \"WGS-84\",\r\n  \"GPSDateStamp\": \"2034:02:01\"\r\n}";

            // act
            var result = await _sut.GetMetadataAsync(_imageFilename).ConfigureAwait(false);

            // assert
            var resultExif = (string)result.EXIF.ToString();
            resultExif.Should().Be(EXPECTED_EXIF);
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }
    }
}