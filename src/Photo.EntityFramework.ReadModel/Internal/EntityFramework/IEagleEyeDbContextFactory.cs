namespace Photo.EntityFramework.ReadModel.Internal.EntityFramework
{
    using System.Threading.Tasks;

    internal interface IEagleEyeDbContextFactory
    {
        Task Initialize();

        EagleEyeDbContext CreateMediaItemDbContext();
    }
}
