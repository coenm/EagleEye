﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using FileImporter.CmdOptions;
using FileImporter.Data;
using FileImporter.Indexing;
using FileImporter.Infrastructure;
using FileImporter.Infrastructure.Everything;
using FileImporter.Infrastructure.FileIndexRepository;
using FileImporter.Infrastructure.PersistantSerializer;
using ShellProgressBar;
using SimpleInjector;

namespace FileImporter
{
    public static class Program
    {
//        private const string RootPath = @"D:\Fotoalbum";
        private const string IndexFilename = @"D:\Fotoalbum\index.json";

        private static Container _container;

        private static readonly ProgressBarOptions ProgressOptions = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };


        public static void Main(string[] args)
        {
            _container = new Container();
//            Startup.ConfigureContainer(_container, rootPath, indexFilename);
            Run(args);
        }

        public static void Run(string[] args)
        {
            Parser.Default.ParseArguments<AutoDeleteSameFile, MoveOptions, UpdateIndexOptions, CheckIndexOptions, SearchOptions, FindAndHandleDuplicatesOptions>(args)
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

            Console.WriteLine("Done. Press enter to exit.");
            Console.ReadLine();
        }

        private static void AutoDeleteSameFile(AutoDeleteSameFile options)
        {
            Startup.ConfigureContainer(_container, "", options.IndexFile);

            var searchService = _container.GetInstance<SearchService>();
            var contentResolver = _container.GetInstance<IContentResolver>();

            var allIdexes = searchService.FindAll().ToArray();

            using (var pbar = new ProgressBar(allIdexes.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in allIdexes)
                {
                    pbar.Tick(index.Identifier);

                    // check if file exists.
                    if (!contentResolver.Exist(index.Identifier))
                    {
                        continue;
                    }

                    var duplicates = allIdexes
                        .Where(f => f != index && f.Hashes.FileHash.SequenceEqual(index.Hashes.FileHash))
                        .ToList();

                    if (!duplicates.Any())
                        continue;

                    var fileInfo = new FileInfo(index.Identifier);
                    var dirIndex = fileInfo.Directory.FullName;

                    var filenameWithoutExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                    var duplicatesInSameDirectory = duplicates
                        .Where(f =>
                        {
                            var info = new FileInfo(f.Identifier);
                            if (info.Directory.FullName != dirIndex)
                                return false;

                            return true;
                            var filenameWithoutExt = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                            if (filenameWithoutExt.StartsWith(filenameWithoutExtension))
                                return true;

                            return false;

                        })
                        .ToList();

                    if (duplicatesInSameDirectory.Any())
                    {
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

            using (var pbar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var file in files)
                {
                    pbar.Tick(file);

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
            SingleFileIndexRepository repo2;
            var rp = string.Empty;
            Startup.ConfigureContainer(_container, rp, options.IndexFile);


            var searchService = _container.GetInstance<SearchService>();
            var everything = new Everything();

            if (string.IsNullOrWhiteSpace(options.DirectoryToIndex))
            {
                if (!File.Exists(options.IndexFiles2))
                {
                    Console.WriteLine("File doesnt exist");
                    return;
                }

                repo2 = new SingleFileIndexRepository(new JsonToFileSerializer<List<FileIndex>>(options.IndexFiles2));


                var allFiles = repo2.Find(f => true).Where(f => File.Exists(f.Identifier)).ToList();


                using (var pbar = new ProgressBar(allFiles.Count, "Initial message", ProgressOptions))
                {
                    foreach (var index in allFiles)
                    {
                        pbar.Tick(index.Identifier);
                        List<FileIndex> similars = searchService.FindSimilar(index).ToList();
                        similars = similars.Where(f => !f.Identifier.Contains("ElSheik")).ToList(); 
                        similars = similars.Where(f => File.Exists(f.Identifier)).ToList();
                        
                        if (similars.Any())
                        {
                            similars.Add(index);
                            everything.Show(similars);
                            Console.WriteLine("Press enter for next");
                            Console.ReadKey();
                        }
                    }
                }

                return;
            }
            else
            {
                if (!Directory.Exists(options.DirectoryToIndex))
                {
                    Console.WriteLine("Directory does not exist.");
                    return;
                }
            }

            var diDirToIndex = new DirectoryInfo(options.DirectoryToIndex).FullName;
            var files = Directory
                .EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories)
                .ToArray();


            var indexService = _container.GetInstance<CalculateIndexService>();

            using (var pbar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in files)
                {
                    pbar.Tick(index);

                    var items = new string[1];
                    items[0] = index;

                    var result = indexService.CalculateIndex(items).Single();
                    List<FileIndex> similars = searchService.FindSimilar(result).ToList();

                    similars = similars.Where(f => !f.Identifier.Contains("ElSheik")).ToList();

                    if (similars.Any())
                    {
                        similars.Add(result);
                        everything.Show(similars);
                        Console.WriteLine("Press enter for next");
                        Console.ReadKey();
                    }
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

            Startup.ConfigureContainer(_container, "", options.OutputFile);

            var searchService = _container.GetInstance<SearchService>();
            var persistantService = _container.GetInstance<PersistantFileIndexService>();
            var contentResolver = _container.GetInstance<IContentResolver>();

            var allIdexes = searchService.FindAll().ToArray();

            using (var pbar = new ProgressBar(allIdexes.Length, "Initial message", ProgressOptions))
            {
                foreach (var index in allIdexes)
                {
                    pbar.Tick(index.Identifier);

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

            Startup.ConfigureContainer(_container, rp, options.OutputFile);


            var files = Directory
                .EnumerateFiles(diDirToIndex, "*.jpg", SearchOption.AllDirectories)
//                .Select(f => ConvertToRelativeFilename(rp, f))
                .ToArray();


            var searchService = _container.GetInstance<SearchService>();
            var indexService = _container.GetInstance<CalculateIndexService>();
            var persistantService = _container.GetInstance<PersistantFileIndexService>();

            using (var pbar = new ProgressBar(files.Length, "Initial message", ProgressOptions))
            {
                foreach (var file in files)
                {
                    pbar.Tick(file);
                    var items = new string[1];
                    items[0] = file;

                    if (options.Force)
                    {
                        // index and add
                        pbar.Message = $"Processing '{file}' ";
                        var index = indexService.CalculateIndex(items);
                        persistantService.AddOrUpdate(index.Single());
                    }
                    else
                    {
                        var foundItem = searchService.FindById(file);
                        if (foundItem == null)
                        {
                            // index and add
                            pbar.Message = $"Processing '{file}' ";
                            var index = indexService.CalculateIndex(items);
                            persistantService.AddOrUpdate(index.Single());
                        }
                    }

                }
            }

            Console.ReadKey();
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

        private static void HandleDuplicates(FileData[] files, FindAndHandleDuplicatesOptions options)
        {
            if (options.DuplicateAction == FileAction.Keep)
                return;


            if (options.DuplicateAction == FileAction.Delete)
            {
                foreach (var file in files)
                {
                    try
                    {
                        if (File.Exists(file.FileName))
                            File.Delete(file.FileName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($" Error deleting file '{file.FileName}'. Ex msg : {e.Message}");
                    }
                }
            }


            if (options.DuplicateAction == FileAction.Move)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var fi = new FileInfo(file.FileName);
                        if (fi.Exists)
                        {
                            var destFileName = Path.Combine(options.DuplicateDir, fi.Name);
                            fi.MoveTo(destFileName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($" Error moving file '{file.FileName}'. Ex msg : {e.Message}");
                    }
                }
            }
        }
    }
}
