namespace EagleEye.Photo.ReadModel.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        private readonly IEagleEyeDbContextFactory dbContextFactory;

        public ModuleInitializer([NotNull] IEagleEyeDbContextFactory dbContextFactory)
        {
            Helpers.Guards.Guard.NotNull(dbContextFactory, nameof(dbContextFactory));
            this.dbContextFactory = dbContextFactory;
        }

        public Task InitializeAsync()
        {
            return dbContextFactory.Initialize();
        }
    }
}
