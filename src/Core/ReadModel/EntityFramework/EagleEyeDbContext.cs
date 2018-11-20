namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework.Models;
    using Microsoft.EntityFrameworkCore;

    internal class EagleEyeDbContext : DbContext
    {
        public EagleEyeDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Photo> Photos { get; set; }

        public Task EnsureCreated() => Database.EnsureCreatedAsync();
    }
}
