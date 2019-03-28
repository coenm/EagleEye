namespace EagleEye.Photo.Domain.Services
{
    using System.Collections.Concurrent;

    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class InMemoryFilenameRepository : IFilenameRepository
    {
        [NotNull] private readonly ConcurrentDictionary<string, object> registeredFileNames;

        public InMemoryFilenameRepository()
        {
            registeredFileNames = new ConcurrentDictionary<string, object>();
        }

        public bool Contains([NotNull] string filename)
        {
            Dawn.Guard.Argument(filename, nameof(filename)).NotNull();
            return registeredFileNames.ContainsKey(filename);
        }

        public void Add([NotNull] string filename)
        {
            Dawn.Guard.Argument(filename, nameof(filename)).NotNull();
            registeredFileNames.TryAdd(filename, null);
        }
    }
}
