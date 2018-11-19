namespace EagleEye.Core.ReadModel.EntityFramework.InMemory
{
    using System;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    [UsedImplicitly]
    public class InMemoryDatabaseOptionsBuilder : IDbContextOptionsStrategy
    {
        private const string Key = "InMemory";

        public int Priority { get; } = 10;

        public bool CanHandle([CanBeNull] string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            if (!connectionString.StartsWith(Key, StringComparison.OrdinalIgnoreCase))
                return false;

            var name = GetNameFromConnectionString(connectionString);
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return true;
        }

        public DbContextOptionsBuilder<MediaItemDbContext> Create([CanBeNull] string connectionString)
        {
            // By contract, the param can be null, however, by design, this strategy only works when it is not null (see CanHandle)
            Guard.NotNull(connectionString, nameof(connectionString));

            return new DbContextOptionsBuilder<MediaItemDbContext>()
                .UseInMemoryDatabase(GetNameFromConnectionString(connectionString));
        }

        [NotNull]
        private string GetNameFromConnectionString([NotNull] string connectionString)
        {
            DebugGuard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            DebugGuard.MustBeGreaterThanOrEqualTo(connectionString.Length, Key.Length, $"{nameof(connectionString)}.{nameof(connectionString.Length)}");

            // todo spaces, semicolons etc. etc?
            return connectionString.Substring(Key.Length).Trim();
        }
    }
}
