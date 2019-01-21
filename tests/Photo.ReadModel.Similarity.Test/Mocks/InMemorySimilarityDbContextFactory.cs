namespace Photo.ReadModel.Similarity.Test.Mocks
{
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;

    internal class InMemorySimilarityDbContextFactory : ISimilarityDbContextFactory, IDisposable
    {
        private readonly DbConnection connection;
        private readonly SimilarityDbContextFactory ctxFactory;

        public InMemorySimilarityDbContextFactory()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SimilarityDbContext>()
                .UseSqlite(connection)
                .Options;

            ctxFactory = new SimilarityDbContextFactory(options);
        }

        public Task Initialize() => ctxFactory.Initialize();

        public ISimilarityDbContext CreateDbContext() => ctxFactory.CreateDbContext();

        public void Dispose() => connection.Dispose();
    }
}
