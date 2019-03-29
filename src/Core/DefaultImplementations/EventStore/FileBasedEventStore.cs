namespace EagleEye.Core.DefaultImplementations.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using Newtonsoft.Json;

    // This is a quick implementation to store the events to json files.
    // No decent error handling, no single responsibility etc but it works
    public class FileBasedEventStore : IEventStore
    {
        private readonly IEventPublisher publisher;
        [NotNull] private readonly string basePath;
        [NotNull] private readonly Dictionary<Guid, List<IEvent>> inMemoryDb = new Dictionary<Guid, List<IEvent>>();
        [NotNull] private readonly JsonSerializerSettings settings;

        public FileBasedEventStore([NotNull] IEventPublisher publisher, [NotNull] string basePath)
        {
            Dawn.Guard.Argument(publisher, nameof(publisher)).NotNull();
            Dawn.Guard.Argument(basePath, nameof(basePath)).NotNull().NotWhiteSpace();
            this.publisher = publisher;
            this.basePath = basePath;
            settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
            };
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var @event in events)
            {
                inMemoryDb.TryGetValue(@event.Id, out var list);
                if (list == null)
                {
                    // check if we need to load from file.
                    list = LoadOrCreate(@event.Id);

                    inMemoryDb.Add(@event.Id, list);
                }

                list.Add(@event);

                SaveToFile(@event.Id, list);

                await publisher.Publish(@event, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            inMemoryDb.TryGetValue(aggregateId, out var events);

            if (events == null)
            {
                events = LoadOrCreate(aggregateId);
                inMemoryDb.Add(aggregateId, events);
            }

            return Task.FromResult(events.Where(x => x.Version > fromVersion));
        }

        private string CreateFilename(Guid eventId)
        {
            return Path.Combine(basePath, $"{eventId.ToString()}.json");
        }

        [NotNull]
        private List<IEvent> LoadOrCreate(Guid guid)
        {
            var filename = CreateFilename(guid);

            if (File.Exists(filename))
            {
                try
                {
                    var txt = File.ReadAllText(filename);
                    return JsonConvert.DeserializeObject<List<IEvent>>(txt, settings);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw e;
                }
            }

            return new List<IEvent>();
        }

        private void SaveToFile(Guid guid, List<IEvent> list)
        {
            try
            {
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
                var serialized = JsonConvert.SerializeObject(list, settings);
                var filename = CreateFilename(guid);
                File.WriteAllText(filename, serialized);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
