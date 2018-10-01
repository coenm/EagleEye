namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CommandLine;

    using EagleEye.FileImporter.CmdOptions;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure;
    using EagleEye.FileImporter.Infrastructure.Everything;
    using EagleEye.FileImporter.Infrastructure.FileIndexRepository;
    using EagleEye.FileImporter.Infrastructure.PersistantSerializer;
    using EagleEye.FileImporter.Similarity;

    using ShellProgressBar;

    using SimpleInjector;

    public static class Program
    {
        private static Container _container;

        private static readonly ProgressBarOptions ProgressOptions = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            // ProgressBarOnBottom = true,
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkYellow,
        };

        private static readonly ProgressBarOptions ChildOptions = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ProgressCharacter = '─'
        };

        public static void Main(string[] args)
        {
            _container = new Container();
//            Startup.ConfigureContainer(_container, rootPath, indexFilename);
            Run(args);
        }

        public static void Run(string[] args)
        {
            Parser.Default.ParseArguments<UpdateSimilarityOptions, AutoDeleteSameFile, MoveOptions, UpdateIndexOptions, CheckIndexOptions, SearchOptions, SearchDuplicateFileOptions, FindAndHandleDuplicatesOptions>(args)
                .WithParsed<UpdateSimilarityOptions>(UpdateSimilarity)
                .WithParsed<SearchDuplicateFileOptions>(SearchDuplicateFile)
                .WithParsed<AutoDeleteSameFile>(AutoDeleteSameFile)
                .WithParsed<MoveOptions>(MoveFiles)
                .WithParsed<UpdateIndexOptions>(UpdateIndex)
                .WithParsed<CheckIndexOptions>(CheckIndex)
                .WithParsed<SearchOptions>(Search)
                .WithParsed<FindAndHandleDuplicatesOptions>(FindAndProcessDuplicates)
                .WithNotParsed(errs =>
                {
                    Console.WriteLine("Could not parse the arguments.");
                });

            Console.WriteLine("Done.");
//            Console.WriteLine("Done. Press enter to exit.");
//            Console.ReadLine();
        }

        private static void UpdateSimilarity(UpdateSimilarityOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.IndexFile))
            {
                Console.WriteLine("IndexFile file cannot be empty.");
                return;
            }

            Startup.ConfigureContainer(_container, options.IndexFile);

            var searchService = _container.GetInstance<SearchService>();
            var indexService = _container.GetInstance<CalculateIndexService>();
            var similarityRepository = _container.GetInstance<SimilarityService>();

            var contentResolver = _container.GetInstance<IContentResolver>();

            var allIndexes = searchService.FindAll().ToArray();

            using (var progressBar = new ProgressBar(allIndexes.Length, "Search duplicates", ProgressOptions))
            {
                using (var subProgressBar = progressBar.Spawn(allIndexes.Length, "", ChildOptions))
                {
                    foreach (var indexedFile in allIndexes)
                    {
                        progressBar.Tick(indexedFile.Identifier);

                        var progress = new Progress<FilenameProgressData>(data =>
                        {
                            subProgressBar.MaxTicks = data.Total;
                            subProgressBar.Tick(data.Filename);
                        });

                        subProgressBar.Tick(0, "start");
                        similarityRepository.Update(indexedFile, progress);
                    }
                }
            }

            Console.WriteLine("Done!");
        }

        private static void SearchDuplicateFile(SearchDuplicateFileOptions options)
        {
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

                //return fi.Identifier.StartsWith(options.PathPrefix);
            };

//            if (string.IsNullOrWhiteSpace(options.PathPrefix))
//            {
//                if (Directory.Exists(options.PathPrefix))
//                    tempSpecialPredicate = fi =>
//                    {
//                        //return fi.Identifier.StartsWith(options.PathPrefix);
//                    };
//            }


            Startup.ConfigureContainer(_container, options.IndexFile);

            var searchService = _container.GetInstance<SearchService>();
            var indexService = _container.GetInstance<CalculateIndexService>();
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
                        similar  = searchService.FindSimilar(index, matchValue, matchValue, matchValue)
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

        private static void AutoDeleteSameFile(AutoDeleteSameFile options)
        {
            Startup.ConfigureContainer(_container, options.IndexFile);

            var searchService = _container.GetInstance<SearchService>();
            var contentResolver = _container.GetInstance<IContentResolver>();

            var allIndexes = searchService.FindAll().ToArray();

            using (var progressBar = new ProgressBar(allIndexes.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in allIndexes)
                {
                    progressBar.Tick(index.Identifier);

                    // check if file exists.
                    if (!contentResolver.Exist(index.Identifier))
                    {
                        continue;
                    }

                    var duplicates = allIndexes
                        .Where(f => f != index && f.Hashes.FileHash.SequenceEqual(index.Hashes.FileHash))
                        .ToList();

                    if (!duplicates.Any())
                        continue;

                    var fileInfo = new FileInfo(index.Identifier);
                    var dirIndex = fileInfo.Directory.FullName;

                    // var filenameWithoutExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                    var duplicatesInSameDirectory = duplicates
                        .Where(f =>
                        {
                            var info = new FileInfo(f.Identifier);
                            if (info.Directory.FullName != dirIndex)
                                return false;

                            return true;
                            // var filenameWithoutExt = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                            // if (filenameWithoutExt.StartsWith(filenameWithoutExtension))
                            //     return true;
                            //
                            // return false;
                        })
                        .ToList();

                    if (!duplicatesInSameDirectory.Any())
                        continue;

                    // remove these
                    foreach (var fileToRemove in duplicatesInSameDirectory)
                    {
                        try
                        {
                            if (File.Exists(fileToRemove.Identifier))
                                File.Delete(fileToRemove.Identifier);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }
        }

        private static void MoveFiles(MoveOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Directory))
            {
                Console.WriteLine("Cannot be null or empty");
                return;
            }

            if (!Directory.Exists(options.Directory))
            {
                Console.WriteLine("Directory does not exist");
                return;
            }

            var directoryInfo = new DirectoryInfo(options.Directory);

            var files = Directory
                .EnumerateFiles(directoryInfo.FullName, "*.*", SearchOption.TopDirectoryOnly)
                .ToArray();

            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var file in files)
                {
                    progressBar.Tick(file);

                    var dt = ExtractDateFromFilename.TryGetFromFilename(file);
                    if (!dt.HasValue)
                    {
                        continue;
                    }

                    var d = Path.Combine(directoryInfo.FullName, dt.Value.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(d))
                    {
                        try
                        {
                            Directory.CreateDirectory(d);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    if (!Directory.Exists(d))
                    {
                        continue;
                    }


                    try
                    {
                        var fi = new FileInfo(file);
                        var destFileName = Path.Combine(d, fi.Name);
                        File.Move(fi.FullName, destFileName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private static void Search(SearchOptions options)
        {
            Startup.ConfigureContainer(_container, options.IndexFile);

            var searchService = _container.GetInstance<SearchService>();
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
                        everything.Show(similar);
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


            var indexService = _container.GetInstance<CalculateIndexService>();

            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in files)
                {
                    progressBar.Tick(index);

                    var items = new string[1];
                    items[0] = index;

                    var result = indexService.CalculateIndex(items).Single();
                    var similars = searchService.FindSimilar(result).ToList();

                    similars = similars.Where(f => !f.Identifier.Contains("ElSheik")).ToList();

                    if (similars.Any())
                        continue;

                    similars.Add(result);
                    everything.Show(similars);
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
        private static void CheckIndex(CheckIndexOptions options)
        {
            // todo input validation
//            var diRoot = new DirectoryInfo(RootPath).FullName;
//            var rp = RootPath;

            Startup.ConfigureContainer(_container, options.OutputFile);

            var searchService = _container.GetInstance<SearchService>();
            var persistantService = _container.GetInstance<PersistantFileIndexService>();
            var contentResolver = _container.GetInstance<IContentResolver>();

            var allIndexes = searchService.FindAll().ToArray();

            using (var progressBar = new ProgressBar(allIndexes.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in allIndexes)
                {
                    progressBar.Tick(index.Identifier);

                    // check if file exists.
                    if (!contentResolver.Exist(index.Identifier))
                    {
                        persistantService.Delete(index.Identifier);
                    }
                }
            }
        }

        private static void UpdateIndex(UpdateIndexOptions options)
        {
            // todo input validation
            if (!Directory.Exists(options.DirectoryToIndex))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            var diDirToIndex = new DirectoryInfo(options.DirectoryToIndex).FullName;
//            var diRoot = new DirectoryInfo(RootPath).FullName;
//
            var rp = string.Empty;
//            if (diDirToIndex.StartsWith(diRoot))
//            {
//                rp = RootPath;
//            }

            Startup.ConfigureContainer(_container, options.OutputFile);

            var files = Directory
                .EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories)
//                .Select(f => ConvertToRelativeFilename(rp, f))
                .ToArray();


            var searchService = _container.GetInstance<SearchService>();
            var indexService = _container.GetInstance<CalculateIndexService>();
            var persistantService = _container.GetInstance<PersistantFileIndexService>();

            using (var progressBar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
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
                        persistantService.AddOrUpdate(index.Single());
                    }
                    else
                    {
                        var foundItem = searchService.FindById(file);
                        if (foundItem != null)
                            continue;

                        // index and add
                        progressBar.Message = $"Processing '{file}' ";
                        var index = indexService.CalculateIndex(items);
                        persistantService.AddOrUpdate(index.Single());
                    }
                }
            }
//            Console.ReadKey();
        }

        private static string ConvertToRelativeFilename(string rootPath, string fullFilename)
        {
            var slnDirectoryLength = rootPath.Length;
            var result = fullFilename.Remove(0, slnDirectoryLength);
            while (result.Length > 0 && (result[0] == '/' || result[0] == '\\'))
                result = result.Substring(1);
            return result;
        }

        private static void FindAndProcessDuplicates(FindAndHandleDuplicatesOptions opts)
        {
            if (string.IsNullOrWhiteSpace(opts.IndexFile))
                ShowError($"Indexfile cannot be null or empty.");

            if (!File.Exists(opts.IndexFile))
                ShowError($"File '{opts.IndexFile}' doesn't exist.");

            if (string.IsNullOrWhiteSpace(opts.ProcessingFile))
                ShowError($"ProcessingFile cannot be null or empty.");

            if (!File.Exists(opts.ProcessingFile))
                ShowError($"File '{opts.ProcessingFile}' doesn't exist.");

            if (opts.DuplicateAction == FileAction.Move)
            {
                if (string.IsNullOrWhiteSpace(opts.DuplicateDir))
                    ShowError($"DuplicateDir cannot be null or empty.");

                if (!Directory.Exists(opts.DuplicateDir))
                    Directory.CreateDirectory(opts.DuplicateDir);
            }

//
//            var index = ReadInputFile(opts.IndexFile);
//            var filesToProcess = ReadInputFile(opts.ProcessingFile);
//
//
//            // find duplicate files
//            var duplicateFiles = filesToProcess
//                .Where(f => index.Any(file => file.Sha256.SequenceEqual(f.Sha256)))
//                .ToArray();
//
//            // find new files
//            var newFiles = filesToProcess.Except(duplicateFiles).ToArray();
//
//
//            HandleDuplicates(duplicateFiles, opts);
//            //HandleNewFiles(newFiles, opts);
//
//            JsonEncoding.WriteDataToJsonFile(duplicateFiles, opts.OutputDuplicateFile);
//            JsonEncoding.WriteDataToJsonFile(newFiles, opts.OutputNewFile);
        }

        private static void ShowError(string s)
        {
            Console.WriteLine(s);
            throw new Exception(s);
        }
    }
}
