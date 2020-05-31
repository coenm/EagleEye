namespace EagleEye.Photo.Domain.Services
{
    using System.Collections.Concurrent;

    using Dawn;
    using JetBrains.Annotations;

    internal class InMemoryMediaFilenameRepository : IMediaFilenameRepository
    {
        [NotNull] private readonly ConcurrentDictionary<string, object> registeredFileNames;

        public InMemoryMediaFilenameRepository()
        {
            registeredFileNames = new ConcurrentDictionary<string, object>();
        }

        public bool Contains([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
            return registeredFileNames.ContainsKey(filename);
        }

        public void Add([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
            registeredFileNames.TryAdd(filename, null);
        }
    }
}
