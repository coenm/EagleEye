using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CommandLine;
using FileImporter.Data;
using FileImporter.Json;

namespace FileImporter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Run(args);
        }


        public static void Run(string[] args)
        {
            // parse options
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            var options = ((Parsed<Options>) parserResult).Value;

            // read input file
            var alreadyProcessedFiles = ReadInputFile(options.InputFile);

            // process directory.
            var processedFiles = ProcessDirectory(options.InputDirectory);

            // find duplicate files
            var duplicateFiles = processedFiles
                .Where(f =>
                    alreadyProcessedFiles.Any(alreadyProcessed => 
                        alreadyProcessed.Sha256.SequenceEqual(f.Sha256)))
                .ToArray();

            // find new files
            var newFiles = processedFiles.Except(duplicateFiles).ToArray();
            
            HandleDuplicates(duplicateFiles, options);

            HandleNewFiles(newFiles, options);

            // save output file
            var result = alreadyProcessedFiles.Concat(newFiles);
            JsonEncoding.WriteDateToJsonFile(result, options.InputFile);
        }

        private static void HandleNewFiles(FileData[] files, Options options)
        {
            // Don't do anything with new files for this moment.
            foreach (var file in files)
                Console.WriteLine($"New file '{file}'");
        }

        private static void HandleDuplicates(FileData[] files, Options options)
        {
            var origBaseDir = Path.Combine(options.InputDirectory);
            var newBaseDir = @"d:\fotos deleted\tmp\";

            // move the file to other directory.
            foreach (var file in files)
            {
                var newFilename = file.FileName.Replace(origBaseDir, newBaseDir);

                var fi = new FileInfo(newFilename);
                
                if (File.Exists(newFilename))
                {
                    Console.WriteLine($"File '{newFilename}' already exists");
                }
                else
                {
                    Console.WriteLine($" - Move file '{file}' to '{newFilename}'");
                    if (!Directory.Exists(fi.DirectoryName))
                        Directory.CreateDirectory(fi.DirectoryName);
                    File.Move(file.FileName, newFilename);
                }
            }
        }


        private static List<FileData> ProcessDirectory(string inputDir)
        {
            if (!Directory.Exists(inputDir))
                throw new Exception("Directory does not exists.");

            var files = Directory
                .EnumerateFiles(inputDir, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.ToLower().EndsWith(".zip"))
                .Where(f => !f.ToLower().EndsWith(".ini"))
                .ToArray();

            var total = files.Length;
            var result = new List<FileData>(total);

            result.AddRange(files.Select(ProcesFile));

            return result;
        }

        private static FileData ProcesFile(string file)
        {
            try
            {
                var fileinfo = new FileInfo(file);
                var result = new FileData
                {
                    FileName = file,
                    SizeInBytes = fileinfo.Length
                };

                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    result.Sha256 = CalculateHash(fileStream);
                }

                return result;
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not procses file '{file}'");
                throw;
            }
        }

        private static List<FileData> ReadInputFile(string filename)
        {
            if (!File.Exists(filename))
                throw new Exception("File doesn't exists.");

            try
            {
                return JsonEncoding.ReadFromFile<List<FileData>>(filename);
            }
            catch (Exception)
            {
                throw new Exception($"Could not read or parse '{filename}'.");
            }
        }

        public static byte[] CalculateHash(Stream stream)
        {
            Debug.Assert(stream != null);
            Debug.Assert(stream.CanSeek);

            stream.Position = 0;

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(stream);
            }
        }
    }
}
