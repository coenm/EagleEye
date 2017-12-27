using System.IO;
using Xunit;

namespace FileImporter.Test
{
    public class DirectoryInfoTest
    {
        [Theory]
        [InlineData("C:/a/b/c")]
        [InlineData("C:\\a\\b\\c")]
        public void AbcTest(string d)
        {
            var di = new DirectoryInfo(d);
            
            Assert.Equal("C:\\a\\b\\c", di.FullName);
        }
    }
}