namespace Photo.EntityFramework.ReadModel.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using Photo.EntityFramework.ReadModel.Internal.EntityFramework.Models;

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
