﻿namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading.Tasks;

    internal interface ISimilarityDbContextFactory
    {
        Task Initialize();

        ISimilarityDbContext CreateDbContext();
    }
}
