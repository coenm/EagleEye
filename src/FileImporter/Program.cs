namespace EagleEye.FileImporter
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
    using EagleEye.FileImporter.CmdOptions;
    using EagleEye.FileImporter.Scenarios.Check;
    using EagleEye.FileImporter.Scenarios.UpdateIndex;
    using EagleEye.FileImporter.Similarity;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface.Model;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using NLog;
    using ShellProgressBar;
    using SimpleInjector;


    public static class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static ConnectionStrings connectionStrings;

        public static async Task Main(string[] args)
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // var baseDirectory = Path.Combine(userDir, "EagleEye", DateTime.Now.ToString("yyyyMMddHHmmss"));
            var baseDirectory = Path.Combine(userDir, "EagleEye", "20200529114747");

            Directory.CreateDirectory(baseDirectory);

            string FullPath(string filename) => Path.Combine(baseDirectory, filename);

            connectionStrings = new ConnectionStrings
                {
                    Similarity = Startup.CreateSqlLiteFileConnectionString(FullPath("Similarity.db")),
                    HangFire = Startup.CreateSqlLiteFileConnectionString(FullPath("Similarity.HangFire.db")),
                    FilenameEventStore = FullPath("EventStore.db"),
                    LuceneDirectory = ConnectionStrings.LuceneInMemory,
                    ConnectionStringPhotoDatabase = "InMemory EagleEye",
                };

            connectionStrings.ConnectionStringPhotoDatabase = Startup.CreateSqlLiteFileConnectionString(FullPath("FullMetadata.db"));
            connectionStrings.LuceneDirectory = FullPath("Lucene");

            await Run(args).ConfigureAwait(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Manual verification.")]
        private static async Task Run(string[] args)
        {
            var task = Task.CompletedTask;

            using var container = Startup.ConfigureContainer(
                connectionStrings,
                new Dictionary<string, object>
                {
                    { "EXIFTOOL_PLUGIN_FULL_EXE", "C:\\Tools\\ExifTool\\exiftool.exe" },
                });

            await Startup.InitializeAllServices(container).ConfigureAwait(false);
            Startup.StartServices(container);

            try
            {
                Parser.Default.ParseArguments<
                        IndexFilesOptions,
                        CheckMetadataOptions,
                        SearchOptions>(args)
                    .WithParsed<IndexFilesOptions>(option => task = Index(container, option))
                    .WithParsed<CheckMetadataOptions>(option => task = CheckMetadata(container, option))
                    .WithParsed<SearchOptions>(option => task = Search(container, option))
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

        private static async Task CheckMetadata([NotNull] Container container, [NotNull] CheckMetadataOptions option)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(option, nameof(option)).NotNull();

            var directoryService = container.GetInstance<IDirectoryService>();

            if (!directoryService.Exists(option.Directory))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var dirToIndex = new DirectoryInfo(option.Directory).FullName;
            var files = GetMediaFiles(directoryService, dirToIndex).ToArray();

            var commandHandler = container.GetInstance<VerifyMediaCommandHandler>();

            var fromSeconds = TimeSpan.FromSeconds(25);

            using var progressBar = new ProgressBar(files.Length, "Initial message", MyProgressBar.ProgressOptions);
            var progress = new Progress<FilenameProgressData>(data =>
                                                              {
                                                                  // progressBar.MaxTicks = data.Total;
                                                                  progressBar.Tick(data.Filename);
                                                              });

            Logger.Info($" Found {files.Length} files.");
            Console.WriteLine($" Found {files.Length} files.");

            var maxDegree = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0));
            Logger.Info($"Max degree {maxDegree}");
            Console.WriteLine($"Max degree {maxDegree}");

            maxDegree = Math.Min(maxDegree, 8);
            maxDegree = Math.Max(maxDegree, 2);
            Logger.Info($"Max degree {maxDegree}");
            Console.WriteLine($"Max degree {maxDegree}");

            IEnumerable<KeyValuePair<string, VerifyMediaResult>> initialCollection = Enumerable.Empty<KeyValuePair<string, VerifyMediaResult>>();
            var results = new ConcurrentDictionary<string, VerifyMediaResult>(maxDegree, initialCollection, null);

            Parallel.ForEach(
                             files,
                             new ParallelOptions { MaxDegreeOfParallelism = maxDegree },
                             file =>
                             {
                                 using var cts = new CancellationTokenSource(fromSeconds);
                                 try
                                 {
                                     VerifyMediaResult result = commandHandler.HandleAsync(file, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                                     results.TryAdd(file, result);
                                     (progress as IProgress<FilenameProgressData>)?.Report(new FilenameProgressData(0, 0, file));
                                 }
                                 catch (OperationCanceledException)
                                 {
                                     Logger.Error("Could not UpdateImporteImage due to timeout.");
                                 }
                                 catch (Exception e)
                                 {
                                     Logger.Warn(e.Message);
                                 }
                             });

            await File.WriteAllTextAsync($"M:\\todo\\output_{DateTime.Now:yyyyMMddHHmmss}.json", JsonConvert.SerializeObject(results)).ConfigureAwait(false);

            var emptyItems = results.Select(x => x.Value).Where(x => x.State == VerifyMediaResultState.NoMetadataAvailable);
            var incorrect = results.Select(x => x.Value).Where(x => x.State == VerifyMediaResultState.MetadataIncorrect);
            var correct = results.Select(x => x.Value).Where(x => x.State == VerifyMediaResultState.MetadataCorrect);

            var metadatas = results.Where(x => x.Value.Metadata != null).Select(x => x.Value).ToArray();
            var duplicateIds = metadatas.Where(x => metadatas.Count(y => y.Metadata.Id == x.Metadata.Id) > 1).ToArray();

            Logger.Info(string.Empty);
            Logger.Info("---- Empty Items ----");
            if (emptyItems.Any())
            {
                foreach (var item in emptyItems)
                {
                    Logger.Info($" - {item.Filename}");
                }
            }
            else
            {
                Logger.Info(" -> none <-");
            }

            Logger.Info(string.Empty);

            Logger.Info(string.Empty);
            Logger.Info("---- Incorrect Items ----");
            if (incorrect.Any())
            {
                foreach (var item in incorrect)
                {
                    Logger.Info($" - {item.Filename}");
                }
            }
            else
            {
                Logger.Info(" -> none <-");
            }

            Logger.Info(string.Empty);

            Logger.Info(string.Empty);
            Logger.Info("---- DuplicateIds Items ----");
            if (duplicateIds.Length > 0)
            {
                foreach (var item in duplicateIds)
                {
                    Logger.Info($" - {item.Filename} {item.Metadata.Id}");
                }
            }
            else
            {
                Logger.Info(" -> none <-");
            }

            Logger.Info(string.Empty);

            Logger.Info(string.Empty);
            Logger.Info("---- Correct Items ----");
            if (correct.Any())
            {
                foreach (var item in correct)
                {
                    Logger.Info($" - {item.Filename}");
                }
            }
            else
            {
                Logger.Info(" -> none <-");
            }

            Logger.Info(string.Empty);
        }

        private static async Task Search([NotNull] Container container, [NotNull] SearchOptions option)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(option, nameof(option)).NotNull();

            var search = container.GetInstance<IReadModel>();

            try
            {
                // search for photos
                var found = await SearchAndShow(search).ConfigureAwait(false);
                bool @continue = true;
                while (found || @continue)
                {
                    found = await SearchAndShow(search).ConfigureAwait(false);
                    if (!found)
                        @continue = AskForNewSearch();
                }

                Console.WriteLine("Stop");

                /*
                https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
                search terms:
                - id
                - version
                - filename
                - filetype
                - city
                - countrycode
                - country
                - state
                - sublocation
                - longitude
                - latitude
                - date
                - person
                - tag
                - gps
                */
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static bool AskForNewSearch()
        {
            Console.WriteLine("Do you want to search something else? (yY)");
            var result = Console.ReadLine();
            var firstChar = result?.FirstOrDefault();
            if (firstChar == null)
                return false;
            if (firstChar == 'y')
                return true;
            if (firstChar == 'Y')
                return true;
            return false;
        }

        private static async Task<bool> SearchAndShow(IReadModel search)
        {
            Console.WriteLine("Enter query and press enter.");
            var query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query))
                return false;

            var searchResults = new List<PhotoResult>(0);

            try
            {
                searchResults = search.FullSearch(query);
                if (searchResults.Count == 0)
                    return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            Console.WriteLine($"Found {searchResults.Count} items.");

            try
            {
                var everything = new Infrastructure.Everything.Everything();
                await everything.Show(searchResults.Select(x => x.Filename)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
            return true;
        }

        private static async Task Index([NotNull] Container container, [NotNull] IndexFilesOptions option)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            Guard.Argument(option, nameof(option)).NotNull();

            var directoryService = container.GetInstance<IDirectoryService>();

            if (!directoryService.Exists(option.Directory))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var dirToIndex = new DirectoryInfo(option.Directory).FullName;
            var files = GetMediaFiles(directoryService, dirToIndex).ToArray();

            var singleExecutor = container.GetInstance<UpdateIndexExecutor>();
            var executor = new UpdateMultipleIndexesExecutor(singleExecutor);

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
