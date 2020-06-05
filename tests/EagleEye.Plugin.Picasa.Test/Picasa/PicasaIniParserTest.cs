namespace EagleEye.Picasa.Test.Picasa
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using FluentAssertions;
    using VerifyXunit;
    using Xunit;
    using Xunit.Abstractions;

    using Sut = EagleEye.Picasa.Picasa.PicasaIniParser;

    public class PicasaIniParserTest : VerifyBase
    {
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

        public PicasaIniParserTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task PicasaFileParserTest()
        {
            // arrange
            await using var stream = GenerateStreamFromString(PicasaIniFileContent);

            // act
            var result = Sut.Parse(stream).ToList();

            // assert
            await Verify(result);
        }

        [Fact]
        public void Parse_ContentWithoutContacts2Section_ShouldReturnEmpty()
        {
            // arrange
            var removedContacts2SectionContent = PicasaIniFileContent.Replace("[Contacts2]", "[Contacts.jpg]");
            using var stream = GenerateStreamFromString(removedContacts2SectionContent);

            // act
            var result = Sut.Parse(stream);

            // assert
            result.Should().BeEmpty();
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }
    }
}
