namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System.Threading.Tasks;

    internal interface IEagleEyeDbContextFactory
    {
        Task Initialize();

        EagleEyeDbContext CreateMediaItemDbContext();
    }
}
