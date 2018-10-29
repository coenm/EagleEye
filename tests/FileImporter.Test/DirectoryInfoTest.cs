namespace EagleEye.FileImporter.Test
{
    using System.IO;

    using EagleEye.TestHelper;
    using Xunit;
    using Xunit.Categories;

    public class DirectoryInfoTest
    {
        [Theory]
        [Exploratory]
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
        [Exploratory]
        [InlineData("/a/b/c")]
        public void LinuxDirectory1Test(string d)
        {
            if (!TestEnvironment.IsLinux)
                return;

            var di = new DirectoryInfo(d);
            Assert.Equal("/a/b/c", di.FullName);
        }

        [Theory]
        [Exploratory]
        [InlineData("\\a\\b\\c")]
        public void LinuxDirectory2Test(string d)
        {
            if (!TestEnvironment.IsLinux)
                return;

            var di = new DirectoryInfo(d);
            Assert.NotEqual("/a/b/c", di.FullName);
        }
    }
}
