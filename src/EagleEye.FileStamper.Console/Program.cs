namespace EagleEye.FileStamper.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CommandLine;
    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.FileStamper.Console.CmdOptions;
    using EagleEye.FileStamper.Console.Scenarios.FixAndUpdateImportImages;
    using JetBrains.Annotations;
    using NLog;
    using SimpleInjector;

    public static class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            await Run(args).ConfigureAwait(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Manual verification.")]
        private static async Task Run(string[] args)
        {
            var task = Task.CompletedTask;

            using var container = Startup.ConfigureContainer(
                new Dictionary<string, object>
                {
                    { "EXIFTOOL_PLUGIN_FULL_EXE", "C:\\Tools\\ExifTool\\exiftool.exe" },
                });

            await Startup.InitializeAllServices(container).ConfigureAwait(false);
            Startup.StartServices(container);

            try
            {
                Parser.Default.ParseArguments<UpdateImportedImagesOptions>(args)
                    .WithParsed<UpdateImportedImagesOptions>(option => task = UpdateImportedImages(container, option))
                    .WithNotParsed(errs => Console.WriteLine($"Could not parse the arguments. {errs.First()}"));

                await task.ConfigureAwait(false);
                Console.WriteLine("Done.Press enter to exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred. Press enter to exit.");
                Console.WriteLine(e);
            }
            finally
            {
                Console.ReadLine();
                Startup.StopServices(container);
            }
        }

        private static async Task UpdateImportedImages([NotNull] Container container, [NotNull] UpdateImportedImagesOptions option)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(option, nameof(option)).NotNull();

            var directoryService = container.GetInstance<IDirectoryService>();

            if (!directoryService.Exists(option.ProcessingDirectory))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var dirToIndex = new DirectoryInfo(option.ProcessingDirectory).FullName;
            var files = GetMediaFiles(directoryService, dirToIndex).ToArray();

            var singleExecutor = container.GetInstance<IUpdateImportImageCommandHandler>();
            var executor = new UpdateMultipleImagesExecutor(singleExecutor);
            using var progressBar = new MyProgressBar(files.Length, "Initial message");

            var progress = new Progress<FileProcessingProgress>(data => progressBar.Update(data));

            await executor.ExecuteAsync(files, progress, CancellationToken.None).ConfigureAwait(false);
        }

        private static IEnumerable<string> GetMediaFiles(IDirectoryService directoryService, string path)
        {
            var filesJpg = directoryService.EnumerateFiles(path, "*.jpg", SearchOption.AllDirectories);
            var filesJpeg = directoryService.EnumerateFiles(path, "*.jpeg", SearchOption.AllDirectories);
            var filesMov = directoryService.EnumerateFiles(path, "*.mov", SearchOption.AllDirectories);
            var filesMp4 = directoryService.EnumerateFiles(path, "*.mp4", SearchOption.AllDirectories);

            // Not supported extensions: avi, mts, wmv
            return filesJpg.Concat(filesJpeg).Concat(filesMov).Concat(filesMp4);
        }
    }
}
