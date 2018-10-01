namespace EagleEye.Core.ReadModel.EntityFramework
{
    public interface IMediaItemDbContextFactory
    {
        MediaItemDbContext CreateMediaItemDbContext();
    }
}