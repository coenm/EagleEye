﻿namespace EagleEye.Core.ReadModel.EntityFramework.SqlLite
{
    using System;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    [UsedImplicitly]
    public class SqlLiteDatabaseOptionsBuilder : IDbContextOptionsStrategy
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

        public DbContextOptionsBuilder<MediaItemDbContext> Create([CanBeNull] string connectionString)
        {
            // By contract, the param can be null, however, by design, this strategy only works when it is not null (see CanHandle)
            Guard.NotNull(connectionString, nameof(connectionString));

            return new DbContextOptionsBuilder<MediaItemDbContext>()
                .UseSqlite(connectionString);
        }
    }
}