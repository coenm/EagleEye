using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileImporter.Indexing;
using Xunit;

namespace FileImporter.Test.Infrastructure.Everything
{
    public class EverythingTest
    {
        //[Fact]
        public async Task ManualTestIfEverythingIsStartedTest()
        {
            var files = new List<string>(5)
            {
                @"D:\fotos todo\2015-09-05 SD Card\2015-08-12 11.48.33.jpg",
                @"D:\fotos todo\2015-09-05 SD Card\2015-07-09 17.35.10.jpg",
                @"D:\fotos todo\2015-09-05 SD Card\2015-07-21 15.01.51.jpg",
                @"D:\fotos todo\2015-09-05 SD Card\2015-08-17 11.20.57.jpg",
                @"D:\fotos todo\2015-09-05 SD Card\2015-09-02 09.30.36.jpg",
            };
            // arrange

            var sut = new FileImporter.Infrastructure.Everything.Everything();
            var fileIndexes = files.Select(p => new ImageData(p)).ToList();

            // act
            await sut.Show(fileIndexes);

            // assert
        }
    }
}