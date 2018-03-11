namespace EagleEye.Picasa
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Picasa.Picasa;

    public class PicasaService : IPicasaService
    {
        private readonly ConcurrentDictionary<string, Task<IEnumerable<FileWithPersons>>> _tasks;
        private readonly object _syncLock = new object();

        public PicasaService()
        {
            _tasks = new ConcurrentDictionary<string, Task<IEnumerable<FileWithPersons>>>();
        }

        public bool CanProvideData(string filename)
        {
            return true;
        }

        public async Task<FileWithPersons> GetDataAsync(string filename)
        {
            var picasafilename = DeterminePicasaFilename(filename);
            var results = await GetOrCreateTask(picasafilename).ConfigureAwait(false);
            return results.FirstOrDefault(x => x.Filename.Equals(filename));
        }

        public void Dispose()
        {
        }

        private Task<IEnumerable<FileWithPersons>> GetOrCreateTask(string picasaFilename)
        {
            lock (_syncLock)
            {
                if (_tasks.TryGetValue(picasaFilename, out var cachedTask))
                    return cachedTask;

                var task = Task.Run(() =>
                                    {
                                        using (var stream = File.OpenRead(picasaFilename))
                                        {
                                            return PicasaIniParser.Parse(stream);
                                        }
                                    });

                _tasks.TryAdd(picasaFilename, task);

                return task;
            }
        }

        private string DeterminePicasaFilename(string mediaFilename)
        {
            return "todo" + mediaFilename;
        }
    }
}