namespace EagleEye.Picasa.Test.Picasa
{
    using System.IO;
    using System.Text;

    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class PicasaContactsXmlReaderTest
    {
        private readonly IFileService fileService;

        public PicasaContactsXmlReaderTest()
        {
            fileService = A.Fake<IFileService>();
        }

        [Fact]
        public void GetContactsFromFile_ShouldReturnContacts_WhenFileContainsContacts()
        {
            // arrange
            var content = @"
<contacts>
 <contact id=""af8a34a6cdcd1b7f"" name=""Ace"" display=""A"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""50a8d85cd1e165c2"" name=""Bear"" display=""B"" modified_time=""2011-05-10T16:34:04+01:00"" local_contact=""1""/>
 <contact id=""40cffd0a1c385555"" name=""Case"" display=""C"" modified_time=""2011-05-10T16:35:04+01:00"" local_contact=""1""/>
</contacts>";

            SetupFileService("dummy", content);
            var sut = new PicasaContactsXmlReader(fileService);

            // act
            var result = sut.GetContactsFromFile("dummy");

            // assert
            result.Should().BeEquivalentTo(
                                           new PicasaContact("af8a34a6cdcd1b7f", "Ace", "A", "2011-05-10T16:33:04+01:00", "1"),
                                           new PicasaContact("50a8d85cd1e165c2", "Bear", "B", "2011-05-10T16:34:04+01:00", "1"),
                                           new PicasaContact("40cffd0a1c385555", "Case", "C", "2011-05-10T16:35:04+01:00", "1"));
        }

        [Fact]
        public void GetContactsFromFile_ShouldSkipContactsWithoutId()
        {
            // arrange
            var content = @"
<contacts>
 <contact id=""af8a34a6cdcd1b7f"" name=""Ace"" display=""A"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""50a8d85cd1e165c2"" name=""Bear"" display=""B"" modified_time=""2011-05-10T16:34:04+01:00"" local_contact=""1""/>
 <contact name=""Case"" display=""C"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
</contacts>";

            SetupFileService("dummy", content);
            var sut = new PicasaContactsXmlReader(fileService);

            // act
            var result = sut.GetContactsFromFile("dummy");

            // assert
            result.Should().BeEquivalentTo(
                                           new PicasaContact("af8a34a6cdcd1b7f", "Ace", "A", "2011-05-10T16:33:04+01:00", "1"),
                                           new PicasaContact("50a8d85cd1e165c2", "Bear", "B", "2011-05-10T16:34:04+01:00", "1"));
        }

        [Fact]
        public void GetContactsFromFile_ShouldSkipContactsWithoutName()
        {
            // arrange
            var content = @"
<contacts>
 <contact id=""af8a34a6cdcd1b7f"" name=""Ace"" display=""A"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""50a8d85cd1e165c2"" name=""Bear"" display=""B"" modified_time=""2011-05-10T16:34:04+01:00"" local_contact=""1""/>
 <contact id=""40cffd0a1c385555"" display=""C"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
</contacts>";

            SetupFileService("dummy", content);
            var sut = new PicasaContactsXmlReader(fileService);

            // act
            var result = sut.GetContactsFromFile("dummy");

            // assert
            result.Should().BeEquivalentTo(
                                           new PicasaContact("af8a34a6cdcd1b7f", "Ace", "A", "2011-05-10T16:33:04+01:00", "1"),
                                           new PicasaContact("50a8d85cd1e165c2", "Bear", "B", "2011-05-10T16:34:04+01:00", "1"));
        }

        private static Stream CreateStream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content ?? string.Empty));
        }

        private void SetupFileService(string filename, string content)
        {
            A.CallTo(() => fileService.FileExists(filename)).Returns(true);
            A.CallTo(() => fileService.OpenRead(filename)).Returns(CreateStream(content));
        }
    }
}
