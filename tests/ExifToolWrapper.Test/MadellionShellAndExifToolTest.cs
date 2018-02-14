namespace EagleEye.ExifToolWrapper.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EagleEye.TestImages;

    using FluentAssertions;

    using Medallion.Shell;

    using Xunit;

    public class MadellionShellAndExifToolTest
    {
        private const string CURRENT_EXIF_TOOL_VERSION = "10.79";
        private const string EXIF_TOOL_EXECUTABLE = "exiftool.exe";
        private readonly string _image;

        // These tests will only run when exiftool is available from PATH.
        public MadellionShellAndExifToolTest()
        {
            _image = Directory
                .GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                .SingleOrDefault();

            _image.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RunExifToolToGetVersion()
        {
            // arrange
            var args = new List<string>
            {
                ExifToolArguments.VERSION
            };

            // act
            var cmd = Command.Run(EXIF_TOOL_EXECUTABLE, args);
            await cmd.Task.ConfigureAwait(false);

            // assert
            cmd.Result.StandardOutput.Should().Be($"{CURRENT_EXIF_TOOL_VERSION}\r\n");
        }

        [Fact]
        public async Task RunExifToolWithCustomStream()
        {
            // arrange
            IEnumerable<string> args = new List<string>
            {
                "-stay_open",
                "True",
                "-@",
                "-",
                "-common_args",
                ExifToolArguments.JSON_OUTPUT,

                // format coordinates as signed decimals.
                "-c",
                "%+.6f",

                "-struct",
                "-g", // group
            };

            var capturedExifToolResults = new Dictionary<string, string>();

            void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
            {
                capturedExifToolResults.Add(dataCapturedArgs.Key, dataCapturedArgs.Data);
            }

            using (var stream = new ExifToolStayOpenStream(new UTF8Encoding()))
            {
                stream.Update += StreamOnUpdate;

                // act
                var cmd = Command.Run(EXIF_TOOL_EXECUTABLE, args).RedirectTo(stream);

                await cmd.StandardInput.WriteLineAsync(ExifToolArguments.VERSION);
                await cmd.StandardInput.WriteLineAsync("-execute0000");
                await cmd.StandardInput.WriteLineAsync(_image);
                await cmd.StandardInput.WriteLineAsync("-execute0005");
                await cmd.StandardInput.WriteLineAsync(_image);
                await cmd.StandardInput.WriteLineAsync("-execute0008");
                await cmd.StandardInput.WriteLineAsync("-stay_open");
                await cmd.StandardInput.WriteLineAsync("False");
                await cmd.Task;

                stream.Update -= StreamOnUpdate;

                // assert
                cmd.Result.Success.Should().BeTrue();
                cmd.Result.StandardError.Should().BeNullOrEmpty();
                capturedExifToolResults.Should().HaveCount(3).And.ContainKeys("0000", "0005", "0008");
            }
        }
    }
}