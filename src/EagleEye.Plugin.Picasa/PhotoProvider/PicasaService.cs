namespace EagleEye.Picasa.PhotoProvider
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

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

        public async Task<FileWithPersons> GetDataAsync(string filename)
        {
            var picasaFilename = DeterminePicasaFilename(filename);
            var results = await GetOrCreateTask(picasaFilename).ConfigureAwait(false);
            return results.FirstOrDefault(item => item.Filename.Equals(Path.GetFileName(filename)));
        }

        public void Dispose()
        {
            // nothing to do
        }

        protected virtual IEnumerable<FileWithPersons> GetFileAndPersonData([NotNull] Stream stream)
        {
            return PicasaIniParser.Parse(stream);
        }

        private Task<IEnumerable<FileWithPersons>> GetOrCreateTask(string picasaFilename)
        {
            lock (syncLock)
            {
                if (tasks.TryGetValue(picasaFilename, out var cachedTask))
                    return cachedTask;

                var task = Task.Run(() =>
                {
                    using (var stream = fileService.OpenRead(picasaFilename))
                    {
                        return GetFileAndPersonData(stream);
                    }
                });

                tasks.TryAdd(picasaFilename, task);
                return task;
            }
        }

        [CanBeNull]
        private string DeterminePicasaFilename([NotNull] string mediaFilename)
        {
            if (string.IsNullOrWhiteSpace(mediaFilename))
                return null;

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
