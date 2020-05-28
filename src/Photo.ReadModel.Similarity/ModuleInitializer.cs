namespace EagleEye.Photo.ReadModel.Similarity
{
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        private readonly ISimilarityDbContextFactory dbContextFactory;

        public ModuleInitializer([NotNull] ISimilarityDbContextFactory dbContextFactory)
        {
            Guard.Argument(dbContextFactory, nameof(dbContextFactory)).NotNull();
            this.dbContextFactory = dbContextFactory;
        }

        public async Task InitializeAsync()
        {
            await dbContextFactory.Initialize().ConfigureAwait(false);
        }
    }
}
