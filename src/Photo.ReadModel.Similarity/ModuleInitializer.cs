﻿namespace EagleEye.Photo.ReadModel.Similarity
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        private readonly ISimilarityDbContextFactory dbContextFactory;

        public ModuleInitializer([NotNull] ISimilarityDbContextFactory dbContextFactory)
        {
            Guard.NotNull(dbContextFactory, nameof(dbContextFactory));
            this.dbContextFactory = dbContextFactory;
        }

        public async Task InitializeAsync()
        {
            await dbContextFactory.Initialize().ConfigureAwait(false);
        }
    }
}