namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Core.Interfaces.Module;

    internal class DatabaseInitializer : IEagleEyeInitialize
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
