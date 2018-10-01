namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    using Microsoft.EntityFrameworkCore;

    public class MediaItemDbContext : DbContext
    {
        public MediaItemDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MediaItemDb> MediaItems { get; set; }

        public Task EnsureCreated() => Database.EnsureCreatedAsync();
    }
}