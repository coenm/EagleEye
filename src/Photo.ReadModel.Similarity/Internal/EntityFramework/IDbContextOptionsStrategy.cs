namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using JetBrains.Annotations;

    using Microsoft.EntityFrameworkCore;

    internal interface IDbContextOptionsStrategy
    {
        int Priority { get; }

        bool CanHandle([CanBeNull] string connectionString);

        DbContextOptionsBuilder<SimilarityDbContext> Create([CanBeNull] string connectionString);
    }
}
