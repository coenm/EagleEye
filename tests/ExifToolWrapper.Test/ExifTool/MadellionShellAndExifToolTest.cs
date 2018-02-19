namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;
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
            ProtectAgainstHangingTask(cmd);
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
                ExifToolArguments.STAY_OPEN,
                ExifToolArguments.BOOL_TRUE,
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

                await cmd.StandardInput.WriteLineAsync(ExifToolArguments.VERSION).ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync("-execute0000").ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync(_image).ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync("-execute0005").ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync(_image).ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync("-execute0008").ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync("-stay_open").ConfigureAwait(false);
                await cmd.StandardInput.WriteLineAsync("False").ConfigureAwait(false);

                ProtectAgainstHangingTask(cmd);
                await cmd.Task.ConfigureAwait(false);

                stream.Update -= StreamOnUpdate;

                // assert
                cmd.Result.Success.Should().BeTrue();
                cmd.Result.StandardError.Should().BeNullOrEmpty();
                capturedExifToolResults.Should().HaveCount(3).And.ContainKeys("0000", "0005", "0008");
            }
        }

        private static void ProtectAgainstHangingTask(Command cmd)
        {
            if (cmd.Task.Wait(TimeSpan.FromSeconds(12)))
                return;

            cmd.Kill();
            throw new Exception("Could not close Exiftool without killing it.");
        }
    }
}