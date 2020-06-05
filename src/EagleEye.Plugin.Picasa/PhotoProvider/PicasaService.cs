namespace EagleEye.Picasa.PhotoProvider
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;

    internal class PicasaService : IPicasaService
    {
        private static readonly string[] PicasaFileNames = { ".picasa.ini", "Picasa.ini" };
        private readonly IFileService fileService;
        private readonly ConcurrentDictionary<string, Task<IEnumerable<FileWithPersons>>> tasks;
        private readonly object syncLock = new object();

        public PicasaService([NotNull] IFileService fileService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();

            this.fileService = fileService;
            tasks = new ConcurrentDictionary<string, Task<IEnumerable<FileWithPersons>>>();
        }

        public bool CanProvideData(string filename)
        {
            if (!fileService.FileExists(filename))
                return false;

            var picasaFilename = DeterminePicasaFilename(filename);
            return picasaFilename != null;
        }

        public async Task<FileWithPersons> GetDataAsync([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            var picasaFilename = DeterminePicasaFilename(filename);
            if (picasaFilename == null)
                return null;

            var results = await GetOrCreateTask(picasaFilename).ConfigureAwait(false);
            return results.FirstOrDefault(item => item.Filename.Equals(Path.GetFileName(filename)));
        }

        public void Dispose()
        {
            // nothing to do
        }

        protected virtual IEnumerable<FileWithPersons> GetFileAndPersonData([NotNull] Stream stream)
        {
            var result = PicasaIniParser.Parse(stream);
            if (result == null)
                return Enumerable.Empty<FileWithPersons>();

            return result.Files ?? Enumerable.Empty<FileWithPersons>();
        }

        private Task<IEnumerable<FileWithPersons>> GetOrCreateTask([NotNull] string picasaFilename)
        {
            Guard.Argument(picasaFilename, nameof(picasaFilename)).NotNull().NotEmpty();

            lock (syncLock)
            {
                if (tasks.TryGetValue(picasaFilename, out var cachedTask))
                    return cachedTask;

                var task = Task.Run(() =>
                    {
                        using var stream = fileService.OpenRead(picasaFilename);
                        return GetFileAndPersonData(stream);
                    });

                tasks.TryAdd(picasaFilename, task);
                return task;
            }
        }

        [CanBeNull]
        private string DeterminePicasaFilename([NotNull] string mediaFilename)
        {
            Guard.Argument(mediaFilename, nameof(mediaFilename)).NotNull().NotEmpty();

            try
            {
                var dirname = new FileInfo(mediaFilename).Directory.FullName;

                foreach (var filename in PicasaFileNames)
                {
                    // Path.GetDirectoryName(path)
                    var picasaIniFilename = Path.Combine(dirname, filename);
                    if (fileService.FileExists(picasaIniFilename))
                        return picasaIniFilename;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
