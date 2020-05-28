namespace EagleEye.EventStore.NEventStoreAdapter
{
    using System;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using JetBrains.Annotations;
    using Microsoft.Data.Sqlite;
    using NEventStore;
    using NEventStore.Persistence.Sql;
    using NEventStore.Persistence.Sql.SqlDialects;
    using NEventStore.Serialization.Json;

    [UsedImplicitly]
    public class NEventStoreAdapterSqliteFactory : INEventStoreAdapterFactory, INEventStoreEventExporterAdapterFactory, IDisposable
    {
        [NotNull] private readonly IStoreEvents store;

        public NEventStoreAdapterSqliteFactory([CanBeNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            var csb = new SqliteConnectionStringBuilder
                      {
                          Mode = SqliteOpenMode.ReadWriteCreate,
                          Cache = SqliteCacheMode.Default,
                          DataSource = filename,
                      };

            store = Wireup.Init()
                          .UseOptimisticPipelineHook()
                          .UsingSqlPersistence(
                              new NetStandardConnectionFactory(
                                  SqliteFactory.Instance,
                                  csb.ToString()))
                          .WithDialect(new SqliteDialect())
                          .InitializeStorageEngine()
                          .UsingJsonSerialization()
                          .Compress()
                          .Build();
        }

        public IEventStore Create([NotNull] IEventPublisher publisher)
        {
            Guard.Argument(publisher, nameof(publisher)).NotNull();

            return new NEventStoreAdapter(publisher, store);
        }

        public void Dispose()
        {
            store.Dispose();
        }

        IEventExporter INEventStoreEventExporterAdapterFactory.Create()
        {
            return new NEventStoreEventExporter(store);
        }
    }
}
