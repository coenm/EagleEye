namespace EagleEye.EventStore.NEventStoreAdapter
{
    using System;

    using CQRSlite.Events;
    using Dawn;
    using JetBrains.Annotations;
    using Microsoft.Data.Sqlite;
    using NEventStore;
    using NEventStore.Persistence.Sql;
    using NEventStore.Persistence.Sql.SqlDialects;
    using NEventStore.Serialization.Json;

    public class NEventStoreAdapterFactory : IDisposable
    {
        [NotNull] private readonly IStoreEvents store;

        public NEventStoreAdapterFactory([CanBeNull] string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                store = Wireup.Init()
                    .UseOptimisticPipelineHook()
                    .UsingInMemoryPersistence()
                    .InitializeStorageEngine()
                    .UsingJsonSerialization()
                    .Build();
            }
            else
            {
                var csb = new SqliteConnectionStringBuilder
                    {
                        Mode = SqliteOpenMode.ReadWriteCreate,
                        Cache = SqliteCacheMode.Default,
                        DataSource = filename,
                    };

                store = Wireup.Init()
                    .UseOptimisticPipelineHook()
                    .UsingSqlPersistence(
                        new NetStandardConnectionFactory(SqliteFactory.Instance, csb.ToString()))
                    .WithDialect(new SqliteDialect())
                    .InitializeStorageEngine()
                    .UsingJsonSerialization()
                    .Compress()
                    .Build();
            }
        }

        public NEventStoreAdapter Create([NotNull] IEventPublisher publisher)
        {
            Guard.Argument(publisher, nameof(publisher)).NotNull();

            return new NEventStoreAdapter(publisher, store);
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
