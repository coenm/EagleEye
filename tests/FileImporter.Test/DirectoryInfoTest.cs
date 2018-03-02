namespace EagleEye.FileImporter.Test
{
    using System.IO;

    using EagleEye.TestImages;

    using Xunit;

    public class DirectoryInfoTest
    {
        [Theory]
        [Xunit.Categories.Exploratory]
        [InlineData("C:/a/b/c")]
        [InlineData("C:\\a\\b\\c")]
        public void WindowsDirectoryTest(string d)
        {
            if (!TestEnvironment.IsWindows)
                return;

            var di = new DirectoryInfo(d);
            Assert.Equal("C:\\a\\b\\c", di.FullName);
        }

        [Theory]
        [Xunit.Categories.Exploratory]
        [InlineData("/a/b/c")]
//        [InlineData("\\a\\b\\c")]
        public void LinuxDirectoryTest(string d)
        {
            if (!TestEnvironment.IsLinux)
                return;

            var di = new DirectoryInfo(d);
            Assert.Equal("/a/b/c", di.FullName);
        }
    }
}