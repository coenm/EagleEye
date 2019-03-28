namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.ContextOptions
{
    using System;

    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    [UsedImplicitly]
    internal class SqlLiteDatabaseOptionsBuilder : IDbContextOptionsStrategy
    {
        private const string Key = "Filename=";

        public int Priority { get; } = 10;

        public bool CanHandle([CanBeNull] string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            if (!connectionString.StartsWith(Key, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        public DbContextOptionsBuilder<EagleEyeDbContext> Create([CanBeNull] string connectionString)
        {
            // By contract, the param can be null, however, by design, this strategy only works when it is not null (see CanHandle)
            Helpers.Guards.Guard.NotNull(connectionString, nameof(connectionString));

            return new DbContextOptionsBuilder<EagleEyeDbContext>()
                .UseSqlite(connectionString);
        }
    }
}
