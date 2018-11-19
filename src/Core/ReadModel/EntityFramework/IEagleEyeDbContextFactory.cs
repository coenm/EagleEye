namespace EagleEye.Core.ReadModel.EntityFramework
{
    public interface IEagleEyeDbContextFactory
    {
        EagleEyeDbContext CreateMediaItemDbContext();
    }
}
