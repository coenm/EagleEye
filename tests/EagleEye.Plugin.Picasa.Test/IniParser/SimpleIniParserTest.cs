namespace EagleEye.Picasa.Test.IniParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    using Sut = EagleEye.Picasa.IniParser.SimpleIniParser;

    public class SimpleIniParserTest
    {
        [Fact]
        public void ParseNullStreamShouldThrowArgumentNullExceptionTest()
        {
            // ReSharper disable once AssignNullToNotNullAttribute, Justification: goal of test.
            Assert.Throws<ArgumentNullException>(() => Sut.Parse(null));
        }

        [Fact]
        public void EmptyIniFileShouldResultInAnEmptyResultTest()
        {
            // arrange
            using var stream = GenerateStreamFromString(string.Empty);

            // act
            var result = Sut.Parse(stream);

            // assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void CommentLinesAreIgnoredTest()
        {
            // arrange
            const string content = @"
[Section1]
  key=value
a = b  
; comment
  
b=c
";

            var expectedContent = new Dictionary<string, string>
                                  {
                                      { "key", "value" },
                                      { "a", "b" },
                                      { "b", "c" },
                                  };

            using var stream = GenerateStreamFromString(content);

            // act
            var result = Sut.Parse(stream);

            // assert
            result.Should().ContainSingle();
            result[0].Section.Should().Be("Section1");
            result[0].Content.Should().BeEquivalentTo(expectedContent);
        }

        [Fact]
        public void Parse_ShouldParseEntriesWithEqualSignAsValue()
        {
            // arrange
            const string content = @"
[filename.jpg]
backuphash=2199
faces=rect64(79291f3aa9295f39),5abc219b7ccc1022
redo=enhance=1;";

            var expectedContent = new Dictionary<string, string>
                                  {
                                      { "backuphash", "2199" },
                                      { "faces", "rect64(79291f3aa9295f39),5abc219b7ccc1022" },
                                      { "redo", "enhance=1;" },
                                  };

            using var stream = GenerateStreamFromString(content);

            // act
            var result = Sut.Parse(stream);

            // assert
            result.Should().ContainSingle();
            result[0].Section.Should().Be("filename.jpg");
            result[0].Content.Should().BeEquivalentTo(expectedContent);
        }

        [Fact]
        public void Parse_ShouldSkipInvalidData()
        {
            // arrange
            const string content = "[Abc\r\nkey=value\r\n";
            using var stream = GenerateStreamFromString(content);

            // act
            var result = Sut.Parse(stream);

            // assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ParsingErrorStreamShouldThrowExceptionTest()
        {
            // arrange
            var s = A.Fake<Stream>();
            A.CallTo(() => s.Read(A<byte[]>._, A<int>._, A<int>._))
             .Throws(new Exception("this is an exception"));

            // act
            Action act = () => Sut.Parse(s);

            // assert
            act.Should().Throw<Exception>().WithMessage("Could not parse stream");
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }
    }
}
