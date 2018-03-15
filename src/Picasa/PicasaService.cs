namespace EagleEye.Picasa
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Picasa.Picasa;

    using JetBrains.Annotations;

    public class PicasaService : IPicasaService
    {
        private static readonly string[] _picasaFilenames = { ".picasa.ini", "Picasa.ini" };
        private readonly IFileService _fileService;
        private readonly ConcurrentDictionary<string, Task<IEnumerable<FileWithPersons>>> _tasks;
        private readonly object _syncLock = new object();

        public PicasaService([NotNull] IFileService fileService)
        {
            _fileService = fileService;
            _tasks = new ConcurrentDictionary<string, Task<IEnumerable<FileWithPersons>>>();
        }

        public bool CanProvideData(string filename)
        {
            if (!_fileService.FileExists(filename))
                return false;

            var picasaFilename = DeterminePicasaFilename(filename);
            return picasaFilename != null;
        }

        public async Task<FileWithPersons> GetDataAsync(string filename)
        {
            var picasafilename = DeterminePicasaFilename(filename);
            var results = await GetOrCreateTask(picasafilename).ConfigureAwait(false);
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
            lock (_syncLock)
            {
                if (_tasks.TryGetValue(picasaFilename, out var cachedTask))
                    return cachedTask;

                var task = Task.Run(() =>
                                    {
                                        using (var stream = _fileService.OpenRead(picasaFilename))
                                        {
                                            return GetFileAndPersonData(stream);
                                        }
                                    });

                _tasks.TryAdd(picasaFilename, task);

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

                foreach (var filename in _picasaFilenames)
                {
                    // Path.GetDirectoryName(path)
                    var picasaIniFilename = Path.Combine(dirname, filename);
                    if (_fileService.FileExists(picasaIniFilename))
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