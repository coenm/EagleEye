namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dawn;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using NLog;

    internal class DbContextOptionsFactory
    {
        [NotNull] private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IEnumerable<IDbContextOptionsStrategy> strategies;

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Used by Guard for null check")]
        public DbContextOptionsFactory([NotNull] IEnumerable<IDbContextOptionsStrategy> strategies)
        {
            Guard.Argument(strategies, nameof(strategies)).NotNull();
            this.strategies = strategies;
        }

        [CanBeNull]
        public DbContextOptions<EagleEyeDbContext> Create([CanBeNull] string connectionString)
        {
            var applicable = strategies
                .OrderBy(x => x.Priority)
                .Where(x => x.CanHandle(connectionString))
                .ToList();

            if (!applicable.Any())
                return null;

            if (applicable.Count > 1)
            {
                Logger.Info(() => $"{applicable.Count} handlers found to create a {nameof(DbContextOptionsBuilder<EagleEyeDbContext>)}. Selecting the first one.");
            }

            return applicable
                .First()
                .Create(connectionString)
                .Options;
        }
    }
}
