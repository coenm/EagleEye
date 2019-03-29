namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.ContextOptions
{
    using System;

    using Dawn;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    [UsedImplicitly]
    internal class InMemoryDatabaseOptionsBuilder : IDbContextOptionsStrategy
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

        public DbContextOptionsBuilder<EagleEyeDbContext> Create([CanBeNull] string connectionString)
        {
            // By contract, the param can be null, however, by design, this strategy only works when it is not null (see CanHandle)
            Guard.Argument(connectionString, nameof(connectionString)).NotNull();

            return new DbContextOptionsBuilder<EagleEyeDbContext>()
                .UseInMemoryDatabase(GetNameFromConnectionString(connectionString));
        }

        [NotNull]
        private string GetNameFromConnectionString([NotNull] string connectionString)
        {
            Guard.Argument(connectionString, nameof(connectionString)).NotNull().NotWhiteSpace().MinLength(Key.Length);

            // todo spaces, semicolons etc. etc?
            return connectionString.Substring(Key.Length).Trim();
        }
    }
}
