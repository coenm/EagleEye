﻿namespace EagleEye.Picasa.Test.Picasa
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
        [Fact]
        public void GetContactsFromFile_ShouldReturnContacts_WhenFileContainsContacts()
        {
            // arrange
            var fileService = A.Fake<IFileService>();
            var content = @"
<contacts>
 <contact id=""af8a34a6cdcd1b7f"" name=""Ace"" display=""A"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""50a8d85cd1e165c2"" name=""Bear"" display=""B"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""40cffd0a1c385555"" name=""Case"" display=""C"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
</contacts>";

            A.CallTo(() => fileService.OpenRead("dummy")).Returns(CreateStream(content));
            var sut = new PicasaContactsXmlReader(fileService);

            // act
            var result = sut.GetContactsFromFile("dummy");

            // assert
            result.Should().BeEquivalentTo(
                                           new PicasaPerson("af8a34a6cdcd1b7f", "Ace"),
                                           new PicasaPerson("50a8d85cd1e165c2", "Bear"),
                                           new PicasaPerson("40cffd0a1c385555", "Case"));
        }

        [Fact]
        public void GetContactsFromFile_ShouldSkipContactsWithoutId()
        {
            // arrange
            var fileService = A.Fake<IFileService>();
            var content = @"
<contacts>
 <contact id=""af8a34a6cdcd1b7f"" name=""Ace"" display=""A"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""50a8d85cd1e165c2"" name=""Bear"" display=""B"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact name=""Case"" display=""C"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
</contacts>";

            A.CallTo(() => fileService.OpenRead("dummy")).Returns(CreateStream(content));
            var sut = new PicasaContactsXmlReader(fileService);

            // act
            var result = sut.GetContactsFromFile("dummy");

            // assert
            result.Should().BeEquivalentTo(
                                           new PicasaPerson("af8a34a6cdcd1b7f", "Ace"),
                                           new PicasaPerson("50a8d85cd1e165c2", "Bear"));
        }

        [Fact]
        public void GetContactsFromFile_ShouldSkipContactsWithoutName()
        {
            // arrange
            var fileService = A.Fake<IFileService>();
            var content = @"
<contacts>
 <contact id=""af8a34a6cdcd1b7f"" name=""Ace"" display=""A"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""50a8d85cd1e165c2"" name=""Bear"" display=""B"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
 <contact id=""40cffd0a1c385555"" display=""C"" modified_time=""2011-05-10T16:33:04+01:00"" local_contact=""1""/>
</contacts>";

            A.CallTo(() => fileService.OpenRead("dummy")).Returns(CreateStream(content));
            var sut = new PicasaContactsXmlReader(fileService);

            // act
            var result = sut.GetContactsFromFile("dummy");

            // assert
            result.Should().BeEquivalentTo(
                                           new PicasaPerson("af8a34a6cdcd1b7f", "Ace"),
                                           new PicasaPerson("50a8d85cd1e165c2", "Bear"));
        }

        private static Stream CreateStream(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content ?? string.Empty));
    }
}