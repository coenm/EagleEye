namespace EagleEye.Core.ReadModel.EntityFramework
{
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    internal interface IDbContextOptionsStrategy
    {
        int Priority { get; }

        bool CanHandle([CanBeNull] string connectionString);

        DbContextOptionsBuilder<EagleEyeDbContext> Create([CanBeNull] string connectionString);
    }
}
