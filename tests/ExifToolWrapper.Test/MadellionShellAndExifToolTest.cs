using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Medallion.Shell;
using TestImages;
using Xunit;

namespace ExifToolWrapper.Test
{
    public class MadellionShellAndExifToolTest
    {
        private readonly string _image;

        private const string CurrentExifToolVersion = "10.25";
        private const string ExifToolExecutable = "exiftool.exe";

        //
        // These tests will only run when exiftool is available from PATH.
        //

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
                ExifToolArguments.Version
            };

            // act
            var cmd = Command.Run(ExifToolExecutable, args);
            await cmd.Task.ConfigureAwait(false);

            // assert
            cmd.Result.StandardOutput.Should().Be($"{CurrentExifToolVersion}\r\n");
        }

        [Fact]
        public async Task RunExifToolWithTextWriter()
        {
            // arrange
            IEnumerable<string> args = new List<string>
            {
                "-stay_open",
                "True",
                "-@",
                "-",
                ExifToolArguments.JsonOutput,
                ExifToolArguments.IgnoreMinorErrorsAndWarnings,
                ExifToolArguments.Quiet,
                ExifToolArguments.Quiet
            };

            var sb = new StringBuilder();
            TextWriter writer = new StringWriter(sb);

            // act
            var cmd = Command.Run(ExifToolExecutable, args).RedirectTo(writer);

            await cmd.StandardInput.WriteLineAsync(ExifToolArguments.Version);
            await cmd.StandardInput.WriteLineAsync("-execute0000");
            await cmd.StandardInput.WriteLineAsync("-stay_open");
            await cmd.StandardInput.WriteLineAsync("False");
            await cmd.Task;

            // assert
            sb.ToString().Should().Be(CurrentExifToolVersion + "\r\n{ready0000}\r\n");
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
                ExifToolArguments.JsonOutput,

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
                var cmd = Command.Run(ExifToolExecutable, args).RedirectTo(stream);

                await cmd.StandardInput.WriteLineAsync(ExifToolArguments.Version);
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