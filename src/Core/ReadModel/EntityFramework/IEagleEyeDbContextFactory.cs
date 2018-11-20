namespace EagleEye.Core.ReadModel.EntityFramework
{
    internal interface IEagleEyeDbContextFactory
    {
        EagleEyeDbContext CreateMediaItemDbContext();
    }
}
