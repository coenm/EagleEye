namespace EagleEye.Photo.ReadModel.Similarity
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        private readonly ISimilarityDbContextFactory dbContextFactory;

        public ModuleInitializer([NotNull] ISimilarityDbContextFactory dbContextFactory)
        {
            Helpers.Guards.Guard.NotNull(dbContextFactory, nameof(dbContextFactory));
            this.dbContextFactory = dbContextFactory;
        }

        public async Task InitializeAsync()
        {
            await dbContextFactory.Initialize().ConfigureAwait(false);
        }
    }
}
