using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileImporter.Picasa;
using Xunit;

using Sut = FileImporter.Picasa.PicasaIniParser;

namespace FileImporter.Test.Picasa
{
    public class PicasIniParserTest
    {
        [Fact]
        public void PicasaFileParserTest()
        {
            // arrange
            using (var stream = GenerateStreamFromString(PicasaIniFileContent))
            {
                // act
                var result = Sut.Parse(stream);

                // assert
                var expectedResult = new List<FileWithPersons>
                {
                    new FileWithPersons("pica 1.jpg", "Calvin", "David", "Alice"),
                    new FileWithPersons("photo 2.jpg", "Alice", "Bob"),
                    new FileWithPersons("nice photo.jpg", "Alice", "Eve Jackson"),
                };

                Assert.Equal(expectedResult.Select(x => x.ToString()), result.Select(x => x.ToString()));
            }
        }

        private const string PicasaIniFileContent = @"
[pica 1.jpg]
backuphash=15125
faces=rect64(a16a5261b7037516),ffffffffffffffff;rect64(397a37cb697a77cb),5bee603ee623542d;rect64(7ba11f6daba15f6d),8ad3eb33cfdf42ce;rect64(a3e041b9d3d081b9),7131e767c91646ae
[Contacts2]
7131e767c91646ae=Alice;;
4759b81b11610b7a=Bob;;
5bee603ee623542d=Calvin;;
8ad3eb33cfdf42ce=David;;
46c60ba61d7cb034=Eve Jackson;;
fd2296b1b887830d=Fred Flinsone;;
646d7c806b7a2c52=Gerald;;
[photo 2.jpg]
faces=rect64(500048f1854a9e46),7131e767c91646ae;rect64(97423e5cc7327e5b),4759b81b11610b7a
backuphash=11571
[nice photo.jpg]
faces=rect64(787e40a89fff7fc0),ffffffffffffffff;rect64(3df8363c8b6289d8),7131e767c91646ae;rect64(7c1f1126c5898483),46c60ba61d7cb034
backuphash=11571";

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

    }
}