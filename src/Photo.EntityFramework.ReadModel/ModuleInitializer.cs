namespace Photo.EntityFramework.ReadModel
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Photo.EntityFramework.ReadModel.Internal.EntityFramework;

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
