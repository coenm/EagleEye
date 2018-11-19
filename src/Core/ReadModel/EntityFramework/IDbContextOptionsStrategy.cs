namespace EagleEye.Core.ReadModel.EntityFramework
{
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    public interface IDbContextOptionsStrategy
    {
        int Priority { get; }

        bool CanHandle([CanBeNull] string connectionString);

        DbContextOptionsBuilder<MediaItemDbContext> Create([CanBeNull] string connectionString);
    }
}
