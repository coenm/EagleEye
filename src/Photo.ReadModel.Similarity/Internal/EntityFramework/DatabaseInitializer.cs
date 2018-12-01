namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;

    internal class DatabaseInitializer : IEagleEyeInitialize
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
