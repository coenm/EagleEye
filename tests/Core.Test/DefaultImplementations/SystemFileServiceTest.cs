namespace EagleEye.Core.Test.DefaultImplementations
{
    using System.Collections.Generic;
    using System.IO;

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.TestHelper;
    using EagleEye.TestHelper.XUnit.Facts;
    using FluentAssertions;
    using Pose;
    using Xunit;

    public class SystemFileServiceTest
    {
        private readonly SystemFileService sut;
        private readonly string filename;

        public SystemFileServiceTest()
        {
            sut = SystemFileService.Instance;
            filename = Path.Combine(TestImages.InputImagesDirectoryFullPath, "1.jpg");
        }

        [Fact]
        public void FileExistsTest()
        {
            // arrange

            // act
            var result = sut.FileExists(filename);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OpenReadTest()
        {
            // arrange
            var fileName = Path.Combine(filename);

            // act
            using var result = sut.OpenRead(fileName);

            // assert
            result.Should().NotBeNull();
        }

        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyor, reason: "Does not work with code coverage")]
        public void OpenWrite_ShouldCallSystemOpenWrite()
        {
            // arrange
            var fileOpenCalled = new List<string>();
            var fileShim = Shim.Replace(() => File.Open(Is.A<string>(), Is.A<FileMode>(), Is.A<FileAccess>(), Is.A<FileShare>()))
                                    .With((string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) =>
                                          {
                                              fileOpenCalled.Add(string.Join(',', path, fileMode, fileAccess, fileShare));
                                              return (FileStream)null; // doesn't matter
                                          });

            // act
            PoseContext.Isolate(() => _ = sut.OpenWrite("test.jpg"), fileShim);

            // assert
            fileOpenCalled.Should().BeEquivalentTo($"test.jpg,{FileMode.Truncate},{FileAccess.Write},{FileShare.Read}");
        }
    }
}
