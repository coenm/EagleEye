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
    using CQRSlite.Commands;
    using Dawn;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.FileImporter.CmdOptions;
    using EagleEye.FileImporter.Json;
    using EagleEye.FileImporter.Scenarios.Check;
    using EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages;
    using EagleEye.FileImporter.Similarity;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.ReadModel.EntityFramework.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NLog;
    using ShellProgressBar;

    using Timestamp = EagleEye.Photo.Domain.Commands.Inner.Timestamp;

    public static class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly ProgressBarOptions ProgressOptions = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkYellow,
        };

        private static readonly ProgressBarOptions ChildOptions = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ProgressCharacter = '─',
        };

        private static ConnectionStrings connectionStrings;

        public static async Task Main(string[] args)
        {
            connectionStrings = new ConnectionStrings
                {
                    Similarity = Startup.CreateSqlLiteFileConnectionString(CreateFullFilename("Similarity.db")),
                    HangFire = Startup.CreateSqlLiteFileConnectionString(CreateFullFilename("Similarity.HangFire.db")),
                    FilenameEventStore = CreateFullFilename("EventStore.db"),
                    LuceneDirectory = ConnectionStrings.LuceneInMemory,
                    ConnectionStringPhotoDatabase = "InMemory EagleEye",
                };

            connectionStrings.ConnectionStringPhotoDatabase = Startup.CreateSqlLiteFileConnectionString(CreateFullFilename("FullMetadata.db"));
            connectionStrings.LuceneDirectory = CreateFullFilename("Lucene");

            await Run(args).ConfigureAwait(false);
        }

        private static async Task Run(string[] args)
        {
            var task = Task.CompletedTask;

            Parser.Default.ParseArguments<
                    IndexFilesOptions,
                    UpdateImportedImagesOptions,
                    CheckMetadataOptions,
                    DemoLuceneSearchOptions>(args)
                .WithParsed<IndexFilesOptions>(option => task = Index(option))
                .WithParsed<UpdateImportedImagesOptions>(option => task = UpdateImportedImages(option))
                .WithParsed<CheckMetadataOptions>(option => task = CheckMetadata(option))
                .WithParsed<DemoLuceneSearchOptions>(option => task = DemoLuceneReadModelSearch(option))
                .WithNotParsed(errs => Console.WriteLine("Could not parse the arguments."));

            try
            {
                await task.ConfigureAwait(false);
                Console.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred. Press enter to exit.");
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static string GetUserDirectory()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userDir, "EagleEye");
        }

        private static string CreateFullFilename([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();

            var baseDir = GetUserDirectory();
            return Path.Combine(baseDir, filename);
        }

        private static async Task CheckMetadata(CheckMetadataOptions option)
        {
            Guard.Argument(option, nameof(option)).NotNull();

            using var container = Startup.ConfigureContainer(connectionStrings);

            await Startup.InitializeAllServices(container);
            Startup.StartServices(container);

            if (!Directory.Exists(option.Directory))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var diDirToIndex = new DirectoryInfo(option.Directory).FullName;

            var xJpg = Directory.EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories);
            var xJpeg = Directory.EnumerateFiles(diDirToIndex, "*.jpeg", SearchOption.AllDirectories);
            var xMov = Directory.EnumerateFiles(diDirToIndex, "*.mov", SearchOption.AllDirectories);
            var xMp4 = Directory.EnumerateFiles(diDirToIndex, "*.mp4", SearchOption.AllDirectories);

            // not supported
            // var xAvi = Directory.EnumerateFiles(diDirToIndex, "*.avi", SearchOption.AllDirectories);
            // var xMts = Directory.EnumerateFiles(diDirToIndex, "*.mts", SearchOption.AllDirectories);
            // var xWmv = Directory.EnumerateFiles(diDirToIndex, "*.wmv", SearchOption.AllDirectories);

            var files = xJpg.Concat(xJpeg).Concat(xMov).Concat(xMp4).ToArray();

            var commandHandler = container.GetInstance<VerifyMediaCommandHandler>();

            var fromSeconds = TimeSpan.FromSeconds(25);

            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                var progress = new Progress<FilenameProgressData>(data =>
                {
                    // progressBar.MaxTicks = data.Total;
                    progressBar.Tick(data.Filename);
                });

                Logger.Info($" Found {files.Length} files.");
                Console.WriteLine($" Found {files.Length} files.");

                var maxDegree = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0));
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

                await File.WriteAllTextAsync($"M:\\todo\\output_{DateTime.Now:yyyyMMddHHmmss}.json", JsonConvert.SerializeObject(results));

                var emptyItems = results.Select(x => x.Value).Where(x => x.State == VerifyMediaResult.VerifyMediaResultState.NoMetadataAvailable);
                var incorrect = results.Select(x => x.Value).Where(x => x.State == VerifyMediaResult.VerifyMediaResultState.MetadataIncorrect);
                var correct = results.Select(x => x.Value).Where(x => x.State == VerifyMediaResult.VerifyMediaResultState.MetadataCorrect);

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
                if (duplicateIds.Any())
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

            Console.WriteLine("DONE");
            container.Dispose();
            Console.ReadKey();
        }

        private static async Task UpdateImportedImages(UpdateImportedImagesOptions option)
        {
            Guard.Argument(option, nameof(option)).NotNull();

            using var container = Startup.ConfigureContainer(connectionStrings);

            await Startup.InitializeAllServices(container);
            Startup.StartServices(container);

            if (!Directory.Exists(option.ProcessingDirectory))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var diDirToIndex = new DirectoryInfo(option.ProcessingDirectory).FullName;

            var xJpg = Directory.EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories);
            var xJpeg = Directory.EnumerateFiles(diDirToIndex, "*.jpeg", SearchOption.AllDirectories);
            var xMov = Directory.EnumerateFiles(diDirToIndex, "*.mov", SearchOption.AllDirectories);
            var xMp4 = Directory.EnumerateFiles(diDirToIndex, "*.mp4", SearchOption.AllDirectories);

            // not supported
            // var xAvi = Directory.EnumerateFiles(diDirToIndex, "*.avi", SearchOption.AllDirectories);
            // var xMts = Directory.EnumerateFiles(diDirToIndex, "*.mts", SearchOption.AllDirectories);
            // var xWmv = Directory.EnumerateFiles(diDirToIndex, "*.wmv", SearchOption.AllDirectories);

            var files = xJpg.Concat(xJpeg).Concat(xMov).Concat(xMp4).ToArray();

            var commandHandler = container.GetInstance<UpdateImportImageCommandHandler>();

            var fromSeconds = TimeSpan.FromSeconds(25);

            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                var progress = new Progress<FilenameProgressData>(data =>
                {
                    // progressBar.MaxTicks = data.Total;
                    progressBar.Tick(data.Filename);
                });

                Logger.Info($" Found {files.Length} files.");
                Console.WriteLine($" Found {files.Length} files.");

                var maxDegree = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0));

                Parallel.ForEach(
                    files,
                    new ParallelOptions { MaxDegreeOfParallelism = maxDegree },
                    file =>
                    {
                        using var cts = new CancellationTokenSource(fromSeconds);
                        try
                        {
                            commandHandler.HandleAsync(file, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
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
            }

            Console.WriteLine("DONE");
            container.Dispose();
            Console.ReadKey();
        }

        private static async Task DemoLuceneReadModelSearch(DemoLuceneSearchOptions option)
        {
            using var container = Startup.ConfigureContainer(connectionStrings);
            await Startup.InitializeAllServices(container);
            Startup.StartServices(container);

            try
            {
                var readModelFacade = container.GetInstance<IReadModelEntityFramework>();
                var dispatcher = container.GetInstance<ICommandSender>();
                var search = container.GetInstance<IReadModel>();

                if (!Directory.Exists(option.Directory))
                {
                    Console.WriteLine("Directory does not exist.");
                    return;
                }

                var diDirToIndex = new DirectoryInfo(option.Directory).FullName;
                var xJpg = Directory.EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories);

                var files = xJpg.Take(1).ToArray();

                // search for photos
                var result = await readModelFacade.GetAllPhotosAsync();

                if (result == null)
                {
                    Console.WriteLine("ReadModel returned null");
                    return;
                }

                if (result.Any() == false)
                {
                    Console.WriteLine("ReadModel returned no items.");
                    return;
                }

                var items = result.ToList();
                Console.WriteLine("Files found:");
                foreach (var item in items)
                    Console.WriteLine($" [{item.Id}] -- ({item.Version}) -- {item.Filename}");

                // https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
                // search terms:
                // - id
                // - version
                // - filename
                // - filetype
                // - city
                // - countrycode
                // - country
                // - state
                // - sublocation
                // - longitude
                // - latitude
                // - date
                // - person
                // - tag
                // - gps

                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.Converters.Add(new StringEnumConverter());
                jsonSerializerSettings.Converters.Add(new Z85ByteArrayJsonConverter());

                var searchQuery = "tag:zoo";
                var searchResults = search.FullSearch(searchQuery);

                searchResults = search.FullSearch(searchQuery);

                if (searchResults.Count > 0)
                {
                    var everything = new FileImporter.Infrastructure.Everything.Everything();
                    await everything.Show(searchResults.Select(x => x.Filename));
                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                }

                Console.WriteLine($"{searchQuery}  --  {searchResults.Count}");
                Console.WriteLine(JsonConvert.SerializeObject(searchResults, Formatting.Indented, jsonSerializerSettings));
                Console.WriteLine();

                searchQuery = "tag:zo~"; // should also match zoo ;-)
                searchResults = search.FullSearch(searchQuery);
                Console.WriteLine($"{searchQuery}  --  {searchResults.Count}");
                Console.WriteLine(JsonConvert.SerializeObject(searchResults, Formatting.Indented, jsonSerializerSettings));
                Console.WriteLine();

                searchQuery = "tag:zo*"; // should also match zoo and zooo ;-)
                searchResults = search.FullSearch(searchQuery);
                Console.WriteLine($"{searchQuery}  --  {searchResults.Count}");
                Console.WriteLine(JsonConvert.SerializeObject(searchResults, Formatting.Indented, jsonSerializerSettings));
                Console.WriteLine();

                searchQuery = "tag:zo"; // should match nothing.
                searchResults = search.FullSearch(searchQuery);
                Console.WriteLine($"{searchQuery}  --  {searchResults.Count}");
                Console.WriteLine(JsonConvert.SerializeObject(searchResults, Formatting.Indented, jsonSerializerSettings));
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine();
                Thread.Sleep(1000 * 5);
                Console.WriteLine("Press enter");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {

                Startup.StopServices(container);
            }

            Console.WriteLine("Press enter");
            Console.ReadKey();
        }

        private static async Task Index(IndexFilesOptions option)
        {
            using var container = Startup.ConfigureContainer(connectionStrings);
            await Startup.InitializeAllServices(container);
            Startup.StartServices(container);

            try
            {
                var dispatcher = container.GetInstance<ICommandSender>();
                var search = container.GetInstance<IReadModel>();

                var mimeTypeProvider = container.GetInstance<IPhotoMimeTypeProvider>();
                var fileHashProvider = container.GetInstance<IFileSha256HashProvider>();
                var tagsProvider = container.GetAllInstances<IPhotoTagProvider>().Single();
                var personsProviders = container.GetAllInstances<IPhotoPersonProvider>().ToArray();
                var photoTakenProviders = container.GetAllInstances<IPhotoDateTimeTakenProvider>().ToArray();
                var photoHashProviders = container.GetAllInstances<IPhotoHashProvider>().ToArray();

                if (!Directory.Exists(option.Directory))
                {
                    Console.WriteLine("Directory does not exist.");
                    return;
                }

                var diDirToIndex = new DirectoryInfo(option.Directory).FullName;
                var xJpg = Directory.EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories);
                var files = xJpg.ToArray();

                using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
                {
                    foreach (var file in files)
                    {
                        // check if file is already in the index  (using lucene search)
                        progressBar.Tick(file);

                        var f = file.Replace(":\\", " ").Replace("\\", " ").Replace("/", " ");
                        var searchFileQuery = "filename:\"" + f + "\"";
                        var count = search.Count(searchFileQuery);

                        if (count > 1)
                        {
                            Logger.Warn($"More than 1 files found. Go to next one. Search query: '{searchFileQuery}'");
                            continue;
                        }

                        var guid = Guid.Empty;

                        if (count == 0)
                        {
                            // add
                            var mimeType = string.Empty;
                            if (mimeTypeProvider.CanProvideInformation(file))
                                mimeType = await mimeTypeProvider.ProvideAsync(file).ConfigureAwait(false);

                            byte[] hash = new byte[32];
                            if (fileHashProvider.CanProvideInformation(file))
                            {
                                var fileHashResult = await fileHashProvider.ProvideAsync(file).ConfigureAwait(false);
                                hash = fileHashResult.ToArray();
                            }

                            var createCommand = new CreatePhotoCommand(file, hash, mimeType);
                            guid = createCommand.Id;
                            await dispatcher.Send(createCommand, CancellationToken.None).ConfigureAwait(false);

                            if (tagsProvider.CanProvideInformation(file))
                            {
                                var tags = await tagsProvider.ProvideAsync(file).ConfigureAwait(false);
                                var updateTagsCommand = new AddTagsToPhotoCommand(guid, null, tags.ToArray());
                                await dispatcher.Send(updateTagsCommand).ConfigureAwait(false);
                            }

                            foreach (var personProvider in personsProviders.OrderBy(x => x.Priority))
                            {
                                if (!personProvider.CanProvideInformation(file))
                                    continue;

                                var persons = await personProvider.ProvideAsync(file).ConfigureAwait(false);
                                var personsCommand = new AddPersonsToPhotoCommand(guid, null, persons.ToArray());
                                await dispatcher.Send(personsCommand).ConfigureAwait(false);
                            }

                            foreach (var photoTakenProvider in photoTakenProviders.OrderBy(x => x.Priority))
                            {
                                if (!photoTakenProvider.CanProvideInformation(file))
                                    continue;

                                var timestamp = await photoTakenProvider.ProvideAsync(file).ConfigureAwait(false);
                                var setDateTimeTakenCommand = new SetDateTimeTakenCommand(guid, null, new Timestamp
                                {
                                    Year = timestamp.Value.Year,
                                    Month = timestamp.Value.Month,
                                    Day = timestamp.Value.Day,
                                    Hour = timestamp.Value.Hour,
                                    Minutes = timestamp.Value.Minute,
                                    Seconds = timestamp.Value.Second,
                                });
                                await dispatcher.Send(setDateTimeTakenCommand).ConfigureAwait(false);
                            }

                            foreach (var photoHashProvider in photoHashProviders.OrderBy(x => x.Priority))
                            {
                                if (!photoHashProvider.CanProvideInformation(file))
                                    continue;

                                var hashes = await photoHashProvider.ProvideAsync(file).ConfigureAwait(false);

                                var tasks = hashes.Select(hash =>
                                {
                                    var updatePhotoHashCommand = new UpdatePhotoHashCommand(guid, null, hash.HashName, hash.Hash);
                                    return dispatcher.Send(updatePhotoHashCommand);
                                });

                                await Task.WhenAll(tasks).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            var guids = search.Search(searchFileQuery);
                            if (guids.Count != 1)
                            {
                                Logger.Warn("Race condition. Not the expected single result happened.");
                                continue;
                            }

                            guid = guids.Single().Id;
                        }
                    }
                }

                Console.WriteLine("Press enter");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Startup.StopServices(container);
            }

            Console.WriteLine("Press enter");
            Console.ReadKey();
        }
    }
}
