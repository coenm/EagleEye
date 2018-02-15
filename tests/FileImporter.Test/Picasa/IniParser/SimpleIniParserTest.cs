namespace EagleEye.FileImporter.Test.Picasa.IniParser
{
    using System;

    using Xunit;

    using Sut = FileImporter.Picasa.IniParser.SimpleIniParser;

    public class SimpleIniParserTest
    {
        [Fact]
        public void ParseNullStreamShouldThrowArgumentNullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => Sut.Parse(null));
        }
    }
}