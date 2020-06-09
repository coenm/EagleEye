namespace EagleEye.Core.Test.DefaultImplementations
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.TestHelper.XUnit.Facts;
    using FluentAssertions;
    using Pose;
    using Xunit;

    public class SystemDirectoryServiceTest
    {
        private readonly SystemDirectoryService sut;

        public SystemDirectoryServiceTest()
        {
            sut = SystemDirectoryService.Instance;
        }

        [ConditionalHostTheory(TestHostMode.Skip, TestHost.AppVeyor, reason: "Does not work with code coverage")]
        [InlineData(true)]
        [InlineData(false)]
        public void Exists_ShouldCallAndReturnSystemDirectoryExists_WhenCalled(bool exists)
        {
            // arrange
            var directoryExistsCalled = new List<string>();
            var directoryShim = Shim.Replace(() => Directory.Exists(Is.A<string>()))
                                    .With((string s) =>
                                          {
                                              directoryExistsCalled.Add(s);
                                              return exists;
                                          });

            // act
            bool result = false;
            PoseContext.Isolate(() => result = sut.Exists("test directory"), directoryShim);

            // assert
            result.Should().Be(exists);
            directoryExistsCalled.Should().BeEquivalentTo("test directory");
        }

        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyor, reason: "Does not work with code coverage")]
        public void EnumerateFiles_ShouldCallAndReturnSystemDirectoryEnumerateFiles_WhenCalled()
        {
            // arrange
            var directoryEnumerateFilesCalled = new List<string>();
            var predefinedResults = new List<string> { "file abc.jpg", "file def.jpg", }.AsEnumerable();
            var directoryShim = Shim.Replace(() => Directory.EnumerateFiles(Is.A<string>(), Is.A<string>(), Is.A<SearchOption>()))
                                    .With((string path, string pattern, SearchOption so) =>
                                          {
                                              directoryEnumerateFilesCalled.Add(string.Join(',', path, pattern, so));
                                              return predefinedResults;
                                          });

            // act
            IEnumerable<string> result = Enumerable.Empty<string>();
            PoseContext.Isolate(() => result = sut.EnumerateFiles("test path", "test search pattern", SearchOption.AllDirectories), directoryShim);

            // assert
            result.Should().BeEquivalentTo(predefinedResults);
            directoryEnumerateFilesCalled.Should().BeEquivalentTo("test path,test search pattern,AllDirectories");
        }
    }
}
