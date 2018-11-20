namespace EagleEye.Core.Test.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Models;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class ExploringEntityFrameworkTests
    {
        [Fact]
        [Xunit.Categories.Exploratory]
        public async Task SelectUsingPredicateFromEmptyDatabaseShouldReturnNothingTest()
        {
            // arrange
            using (var ctx = await CreateMediaItemDbContext())
            {
                // act
                var result = await ctx.Photos
                    .Where(item => item.EventTimestamp <= DateTimeOffset.UtcNow)
                    .ToListAsync()
                    .ConfigureAwait(false);

                // assert
                result.Should().BeEmpty();
            }
        }

        [Fact]
        [Xunit.Categories.Exploratory]
        public async Task SelectUsingPredicateShouldReturnAskedItemTest()
        {
            // arrange
            var item1 = Create(1, "abc1", DateTimeOffset.UtcNow); // should match predicate
            var item2 = Create(2, "abc2", DateTimeOffset.UtcNow); // should match predicate
            var item3 = Create(3, "abc3", DateTimeOffset.UtcNow.AddDays(1)); // should NOT match predicate

            using (var ctx = await CreateMediaItemDbContext())
            {
                await ctx.Photos.AddRangeAsync(item1, item2, item3).ConfigureAwait(false);
                await ctx.SaveChangesAsync().ConfigureAwait(false);

                // act
                var result = await ctx.Photos
                    .Where(item => item.EventTimestamp <= DateTimeOffset.UtcNow)
                    .ToListAsync()
                    .ConfigureAwait(false);

                // assert
                result.Should().BeEquivalentTo(new[] {item1, item2}.AsEnumerable());
            }
        }

        [Fact]
        [Xunit.Categories.Exploratory]
        public async Task InsertWithRelationTest()
        {
            // arrange
            var item1 = Create(1, "aaa", DateTimeOffset.UtcNow);
            item1.Tags = new List<Tag>
            {
                new Tag { Value = "Vacation" },
                new Tag { Value = "Summer" },
            };

            var item2 = Create(1, "bbb", DateTimeOffset.UtcNow);
            item2.Tags = new List<Tag>
            {
                new Tag { Value = "Vacation" },
                new Tag { Value = "Winter" },
            };

            using (var ctx = await CreateMediaItemDbContext())
            {
                await ctx.Photos.AddRangeAsync(item1, item2).ConfigureAwait(false);
                await ctx.SaveChangesAsync().ConfigureAwait(false);

                // act
                var result = await ctx.Photos
                    .ToListAsync()
                    .ConfigureAwait(false);

                // assert
                result.Should().BeEquivalentTo(new[] {item1, item2}.AsEnumerable());
            }
        }

        private static Photo Create(int version, string filename, DateTimeOffset timestamp)
        {
            return new Photo
            {
                Id = Guid.NewGuid(),
                Filename = filename,
                Version = version,
                FileSha256 = new byte[1] { 0x01 },
                EventTimestamp = timestamp,
            };
        }

        private async Task<EagleEyeDbContext> CreateMediaItemDbContext()
        {
            var sut = new InMemoryEagleEyeDbContextFactory();
            var db = sut.CreateMediaItemDbContext();

            // todo, move this
            // because it is Sql Lite InMemory.
            await db.Database.OpenConnectionAsync();
            await db.Database.EnsureCreatedAsync();

            return db;
        }

        internal class InMemoryEagleEyeDbContextFactory : EagleEyeDbContextFactory
        {
            public InMemoryEagleEyeDbContextFactory()
                : base(new DbContextOptionsBuilder<EagleEyeDbContext>()
                    // .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .UseSqlite("Data Source=:memory:;")
                    .Options)
            {
            }
        }
    }
}
