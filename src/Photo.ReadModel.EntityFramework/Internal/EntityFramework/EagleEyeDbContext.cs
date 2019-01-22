namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using Microsoft.EntityFrameworkCore;

    internal class EagleEyeDbContext : DbContext
    {
        public EagleEyeDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Photo> Photos { get; set; }

        public Task EnsureCreatedAsync() => Database.EnsureCreatedAsync();
    }
}
