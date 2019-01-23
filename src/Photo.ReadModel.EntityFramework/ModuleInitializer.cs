namespace EagleEye.Photo.ReadModel.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        private readonly IEagleEyeDbContextFactory dbContextFactory;

        public ModuleInitializer([NotNull] IEagleEyeDbContextFactory dbContextFactory)
        {
            Guard.NotNull(dbContextFactory, nameof(dbContextFactory));
            this.dbContextFactory = dbContextFactory;
        }

        public Task InitializeAsync()
        {
            return dbContextFactory.Initialize();
        }
    }
}
