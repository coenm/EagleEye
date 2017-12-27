using System;
using FileImporter.Infrastructure;
using Xunit;

namespace FileImporter.Test.Infrastructure
{
    public class ExtractDateFromFilenameTest
    {
        [Theory]
        [InlineData("IMG-20170325-WA0014.jpg", 2017, 3, 25)]
        [InlineData("VID-20161220-WA0001.mp4", 2016, 12, 20)]
        [InlineData("20150905_183425.jpg", 2015, 09, 05)]
        public void TryGetFromFilenameTest(string filename, int year, int month, int day)
        {
            // arrange
            var expectedResult = new DateTime(year, month, day, 0, 0, 0);
            
            // act
            var result = ExtractDateFromFilename.TryGetFromFilename(filename);

            // assert
            Assert.Equal(expectedResult, result);
        }
    }
}