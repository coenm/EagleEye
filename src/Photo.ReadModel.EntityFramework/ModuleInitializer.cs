﻿namespace EagleEye.Photo.ReadModel.EntityFramework
{
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        private readonly IEagleEyeDbContextFactory dbContextFactory;

        public ModuleInitializer([NotNull] IEagleEyeDbContextFactory dbContextFactory)
        {
            Guard.Argument(dbContextFactory, nameof(dbContextFactory)).NotNull();
            this.dbContextFactory = dbContextFactory;
        }

        public Task InitializeAsync()
        {
            return dbContextFactory.Initialize();
        }
    }
}
