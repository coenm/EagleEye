namespace EagleEye.Picasa.Test.Picasa
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;
    using VerifyXunit;
    using Xunit;
    using Xunit.Abstractions;

    [UsesVerify]
    public class PicasaContactsXmlWriterTest
    {
        [NotNull] private readonly ITestOutputHelper output;

        public PicasaContactsXmlWriterTest([NotNull] ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Write_ShouldWriteXmlToStream()
        {
            // arrange
            var sut = new PicasaContactsXmlWriter();
            var items = new List<PicasaContact>
                        {
                            new PicasaContact("af8a34a6cdcd1b7f", "Ace", "A", "2011-05-10T16:33:04+01:00", "1"),
                            new PicasaContact("50a8d85cd1e165c2", "Bear", "B", "2011-05-11T16:34:04+01:00", "0"),
                            new PicasaContact("40cffd0a1c385555", "Case", "C", "2011-05-10T16:35:04+01:00", null),
                        };

            // act
            await using var stream = new MemoryStream();
            sut.Write(items, stream);

            // assert
            await Verifier.Verify(Encoding.UTF8.GetString(stream.ToArray()));
        }
    }
}
