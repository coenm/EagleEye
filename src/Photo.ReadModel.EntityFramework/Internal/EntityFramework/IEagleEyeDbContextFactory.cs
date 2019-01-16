namespace Photo.ReadModel.EntityFramework.Internal.EntityFramework
{
    using System.Threading.Tasks;

    internal interface IEagleEyeDbContextFactory
    {
        Task Initialize();

        EagleEyeDbContext CreateMediaItemDbContext();
    }
}
