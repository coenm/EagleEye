namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CommandLine;
    using CQRSlite.Commands;
    using Dawn;
    using EagleEye.FileImporter.CmdOptions;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.Everything;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.PersistentSerializer;
    using EagleEye.FileImporter.Json;
    using EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.ReadModel.EntityFramework.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NLog;
    using ShellProgressBar;

    using Timestamp = EagleEye.Photo.Domain.Commands.Inner.Timestamp;

#pragma warning disable SA1512 // Single-line comments should not be followed by blank line
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1005 // Single line comments should begin with single space
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
                    IndexFile = Path.GetTempFileName(),
                    HangFire = Startup.CreateSqlLiteFileConnectionString(Startup.CreateFullFilename("Similarity.HangFire.db")),
                    FilenameEventStore = Startup.CreateFullFilename("EventStore.db"),
                };

            await Run(args).ConfigureAwait(false);
        }

        private static async Task Run(string[] args)
        {
            var task = Task.CompletedTask;

            Parser.Default.ParseArguments<
                    UpdateImportedImagesOptions,
                    UpdateIndexOptions,
                    CheckIndexOptions,
                    SearchOptions,
                    SearchDuplicateFileOptions,
                    ListReadModelOptions>(args)
                .WithParsed<UpdateImportedImagesOptions>(option => task = UpdateImportedImages(option))
                .WithParsed<SearchDuplicateFileOptions>(option => task = SearchDuplicateFile(option))
                .WithParsed<UpdateIndexOptions>(option => task = UpdateIndex(option))
                .WithParsed<CheckIndexOptions>(option => task = CheckIndex(option))
                .WithParsed<SearchOptions>(option => task = Search(option))
                .WithParsed<ListReadModelOptions>(option => task = ListAllReadModel(option))
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

            var fromSeconds = TimeSpan.FromSeconds(5);
            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var file in files)
                {
                    progressBar.Tick(file);

                    using var cts = new CancellationTokenSource(fromSeconds);

                    try
                    {
                        await commandHandler.HandleAsync(file, cts.Token).ConfigureAwait(false);
                        await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Error("Could not UpdateImporteImage due to timeout.");
                    }
                }
            }

            Console.WriteLine("DONE");
            container.Dispose();
            Console.ReadKey();
        }

        private static async Task ListAllReadModel(ListReadModelOptions opts)
        {
            connectionStrings.IndexFile = opts.IndexFile;
            using var container = Startup.ConfigureContainer(connectionStrings);
            await Startup.InitializeAllServices(container);
            Startup.StartServices(container);

            try
            {
                var readModelFacade = container.GetInstance<IReadModelEntityFramework>();
                var dispatcher = container.GetInstance<ICommandSender>();
                var search = container.GetInstance<IReadModel>();

                var command = new CreatePhotoCommand($"file abc {DateTime.Now}", new byte[32], "image/jpeg", new[] { "zoo", "holiday" }, null);
                await dispatcher.Send(command, CancellationToken.None);

                var commandDateTime = new SetDateTimeTakenCommand(command.Id, null, Timestamp.Create(2010, 04));
                await dispatcher.Send(commandDateTime);

                ICommand tagCommand = new RemoveTagsFromPhotoCommand(command.Id, null, "zoo");
                await dispatcher.Send(tagCommand);

                tagCommand = new AddTagsToPhotoCommand(command.Id, null, "zooo");
                await dispatcher.Send(tagCommand);

                var commandUpdateHash = new UpdatePhotoHashCommand(command.Id, null, "DingDong", 324);
                await dispatcher.Send(commandUpdateHash);

                command = new CreatePhotoCommand($"file abcd {DateTime.Now}", new byte[32], "image/jpeg", new[] { "zoo", "holiday" }, null);
                await dispatcher.Send(command, CancellationToken.None);

                commandDateTime = new SetDateTimeTakenCommand(command.Id, null, Timestamp.Create(2010, 04));
                await dispatcher.Send(commandDateTime);

                commandUpdateHash = new UpdatePhotoHashCommand(command.Id, null, "DingDong", 2343434);
                await dispatcher.Send(commandUpdateHash);

                await Task.Delay(1000);

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

    //             // add names
    //             var others = items.Where(x => x.Id != command.Id);
    //             if (others.Any())
    //             {
    //                 var photo = others.First();
    //                 var command1 = new AddPersonsToPhotoCommand(photo.Id, photo.Version, "AAA11", "BBB11");
    //                 dispatcher.Send(command1).GetAwaiter().GetResult();
    //             }
    //             else
    //             {
    // //                var command1 = new AddPersonsToPhotoCommand(command.Id, 2, "AAA", "BBB");
    //                 var command1 = new UpdatePhotoHashCommand(command.Id, 2, "DingDong", new byte[32]);
    //                 dispatcher.Send(command1).GetAwaiter().GetResult();
    //             }

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
            finally
            {
                Startup.StopServices(container);
            }

            Console.WriteLine("Press enter");
            Console.ReadKey();

            container.Dispose();
        }

        private static async Task SearchDuplicateFile(SearchDuplicateFileOptions options)
        {
            await Task.Yield(); // stupid ;-)

            if (string.IsNullOrWhiteSpace(options.OriginalImageFile))
            {
                Console.WriteLine("Image file cannot be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(options.IndexFile))
            {
                Console.WriteLine("IndexFile file cannot be empty.");
                return;
            }

            var filesToProcess = new List<string>();
            if (File.Exists(options.OriginalImageFile))
            {
                filesToProcess.Add(options.OriginalImageFile);
            }
            else
            {
                if (!Directory.Exists(options.OriginalImageFile))
                {
                    Console.WriteLine("OriginalImage file doesn't exist.");
                    return;
                }

                var diDirToIndex = new DirectoryInfo(options.OriginalImageFile).FullName;
                filesToProcess = Directory
                    .EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories)
                    .ToList();
            }

            if (!filesToProcess.Any())
            {
                Console.WriteLine("No files found.");
                return;
            }

            if (!File.Exists(options.IndexFile))
            {
                Console.WriteLine("Index file doesn't exist.");
                return;
            }

            Func<ImageData, bool> tempSpecialPredicate = fi => true;
            tempSpecialPredicate = fi =>
            {
                if (!fi.Identifier.StartsWith(@"D:\Fotoalbum"))
                    return true;

                var matchYear = false;
                matchYear |= fi.Identifier.Contains("2015");
                matchYear |= fi.Identifier.Contains("2016");
                matchYear |= fi.Identifier.Contains("2017");
                matchYear |= fi.Identifier.Contains("2018");
                return matchYear;

                // return fi.Identifier.StartsWith(options.PathPrefix);
            };

            //            if (string.IsNullOrWhiteSpace(options.PathPrefix))
            //            {
            //                if (Directory.Exists(options.PathPrefix))
            //                    tempSpecialPredicate = fi =>
            //                    {
            //                        //return fi.Identifier.StartsWith(options.PathPrefix);
            //                    };
            //            }
            connectionStrings.IndexFile = options.IndexFile;
            using var container = Startup.ConfigureContainer(connectionStrings);

            var searchService = container.GetInstance<SearchService>();
            var indexService = container.GetInstance<CalculateIndexService>();
            var everything = new Everything();
            var show = false;

            using (var progressBar = new ProgressBar(filesToProcess.Count, "Search duplicates", ProgressOptions))
            {
                foreach (var file in filesToProcess)
                {
                    progressBar.Tick(file);
                    if (!File.Exists(file))
                        continue;

                    var files = new[] { file };
                    var index = indexService.CalculateIndex(files).Single();

                    var found = int.MaxValue;
                    var lastSimilar = new List<ImageData>();
                    var similar = new List<ImageData>();
                    var matchValue = options.Value;

                    while (found > 10 && matchValue <= 100)
                    {
                        lastSimilar = similar;
                        similar = searchService.FindSimilar(index, matchValue, matchValue, matchValue)
                            .Where(f => f.Identifier != index.Identifier
                                        &&
                                        File.Exists(f.Identifier)
                                        &&
                                        tempSpecialPredicate(f))
                            .ToList();
                        found = similar.Count;
                        matchValue++;
                    }

                    if (!similar.Any())
                        similar = lastSimilar;

                    if (!similar.Any())
                        continue;

                    if (show)
                        Console.ReadKey();

                    similar.Add(index);
                    everything.Show(similar);
                    show = true;
                }
            }

            Console.WriteLine("DONE.");
        }

        private static async Task Search(SearchOptions options)
        {
            await Task.Yield(); // stupid ;-)

            connectionStrings.IndexFile = options.IndexFile;
            using var container = Startup.ConfigureContainer(connectionStrings);

            var searchService = container.GetInstance<SearchService>();
            var everything = new Everything();

            if (string.IsNullOrWhiteSpace(options.DirectoryToIndex))
            {
                if (!File.Exists(options.IndexFiles2))
                {
                    Console.WriteLine("File doesn't exist");
                    return;
                }

                var repo2 = new SingleImageDataRepository(new JsonToFileSerializer<List<ImageData>>(options.IndexFiles2));

                var allFiles = repo2.Find(f => true).Where(f => File.Exists(f.Identifier)).ToList();

                using (var progressBar = new ProgressBar(allFiles.Count, "Initial message", ProgressOptions))
                {
                    foreach (var index in allFiles)
                    {
                        progressBar.Tick(index.Identifier);
                        var similar = searchService.FindSimilar(index).ToList();
                        similar = similar.Where(f => !f.Identifier.Contains("ElSheik")).ToList();
                        similar = similar.Where(f => File.Exists(f.Identifier)).ToList();

                        if (!similar.Any())
                            continue;

                        similar.Add(index);
                        await everything.Show(similar);
                        Console.WriteLine("Press enter for next");
                        Console.ReadKey();
                    }
                }

                return;
            }

            if (!Directory.Exists(options.DirectoryToIndex))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var diDirToIndex = new DirectoryInfo(options.DirectoryToIndex).FullName;
            var files = Directory
                .EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories)
                .ToArray();

            var indexService = container.GetInstance<CalculateIndexService>();

            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in files)
                {
                    progressBar.Tick(index);

                    var items = new string[1];
                    items[0] = index;

                    var result = indexService.CalculateIndex(items).Single();
                    var similarItems = searchService.FindSimilar(result).ToList();

                    similarItems = similarItems.Where(f => !f.Identifier.Contains("ElSheik")).ToList();

                    if (similarItems.Any())
                        continue;

                    similarItems.Add(result);
                    await everything.Show(similarItems);
                    Console.WriteLine("Press enter for next");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("DONE");
            Console.ReadKey();
        }

        /// <summary>
        /// Remove index if file does not exist anymore.
        /// </summary>
        private static async Task CheckIndex(CheckIndexOptions options)
        {
            // todo input validation
            //            var diRoot = new DirectoryInfo(RootPath).FullName;
            //            var rp = RootPath;

            await Task.Yield(); // stupid ;-)

            using var container = Startup.ConfigureContainer(connectionStrings);

            var searchService = container.GetInstance<SearchService>();
            var persistentService = container.GetInstance<PersistentFileIndexService>();
            var contentResolver = container.GetInstance<EagleEye.Core.Interfaces.Core.IFileService>();

            var allIndexes = searchService.FindAll().ToArray();

            using var progressBar = new ProgressBar(allIndexes.Length, "Initial message", ProgressOptions);

            foreach (var index in allIndexes)
            {
                progressBar.Tick(index.Identifier);

                // check if file exists.
                if (!contentResolver.FileExists(index.Identifier))
                {
                    persistentService.Delete(index.Identifier);
                }
            }
        }

        private static async Task UpdateIndex(UpdateIndexOptions options)
        {
            await Task.Yield(); // stupid ;-)

            // todo input validation
            if (!Directory.Exists(options.DirectoryToIndex))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var diDirToIndex = new DirectoryInfo(options.DirectoryToIndex).FullName;

            var rp = string.Empty;
            //            if (diDirToIndex.StartsWith(diRoot))
            //            {
            //                rp = RootPath;
            //            }

            using var container = Startup.ConfigureContainer(connectionStrings);

            var files = Directory
                .EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories)
//                .Select(f => ConvertToRelativeFilename(rp, f))
                .ToArray();

            var searchService = container.GetInstance<SearchService>();
            var indexService = container.GetInstance<CalculateIndexService>();
            var persistentService = container.GetInstance<PersistentFileIndexService>();

            using var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions);

            foreach (var file in files)
            {
                progressBar.Tick(file);
                var items = new string[1];
                items[0] = file;

                if (options.Force)
                {
                    // index and add
                    progressBar.Message = $"Processing '{file}' ";
                    var index = indexService.CalculateIndex(items);
                    persistentService.AddOrUpdate(index.Single());
                }
                else
                {
                    var foundItem = searchService.FindById(file);
                    if (foundItem != null)
                        continue;

                    // index and add
                    progressBar.Message = $"Processing '{file}' ";
                    var index = indexService.CalculateIndex(items);
                    persistentService.AddOrUpdate(index.Single());
                }
            }
        }

        private static void ShowError(string s)
        {
            Console.WriteLine(s);
            throw new Exception(s);
        }
    }
#pragma warning restore SA1005 // Single line comments should begin with single space
#pragma warning restore SA1515 // Single-line comment should be preceded by blank line
#pragma warning restore SA1512 // Single-line comments should not be followed by blank line
}
