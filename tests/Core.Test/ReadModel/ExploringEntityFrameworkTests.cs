namespace EagleEye.Core.Test.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Core.ReadModel.EntityFramework.ContextOptions;
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
            var sut = new InMemoryEagleEyeDbContextFactory();

            using (var db = sut.CreateMediaItemDbContext())
            {
                // act
                var result = await db.Photos
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

            var sut = new InMemoryEagleEyeDbContextFactory();

            using (var db = sut.CreateMediaItemDbContext())
            {
                await db.Photos.AddRangeAsync(item1, item2, item3).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);

                // act
                var result = await db.Photos
                                     .Where(item => item.EventTimestamp <= DateTimeOffset.UtcNow)
                                     .ToListAsync()
                                     .ConfigureAwait(false);

                // assert
                result.Should().BeEquivalentTo(new[] { item1, item2 }.AsEnumerable());
            }
        }

        [Fact]
        [Xunit.Categories.Exploratory]
        public async Task Abc()
        {
            // arrange
            var item1 = Create(1, "aaa", DateTimeOffset.UtcNow);
            item1.Tags = new List<Tag>
            {
                new Tag { Value = "Vacation" },
                new Tag { Value = "Summer" },
            };

            var item2 = Create(1, "abcd", DateTimeOffset.UtcNow);
            item2.Tags = new List<Tag>
            {
                new Tag { Value = "Vacation" },
                new Tag { Value = "Winter" },
            };

            var sut = new InMemoryEagleEyeDbContextFactory();
            using (var db = sut.CreateMediaItemDbContext())
//            using (var db = CreateContext())
            {
                await db.EnsureCreated();

                await db.Photos.AddRangeAsync(item1, item2).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);

                // act
                var result = await db.Photos
                    .ToListAsync()
                    .ConfigureAwait(false);

                // assert
                result.Should().BeEquivalentTo(new[] { item1, item2 }.AsEnumerable());
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

        private EagleEyeDbContext CreateContext()
        {
            var x = new DbContextOptionsFactory(new IDbContextOptionsStrategy[] {new InMemoryDatabaseOptionsBuilder(), new SqlLiteDatabaseOptionsBuilder() });
            var xy = x.Create("Filename=./coenm.db");
            var z = new EagleEyeDbContextFactory(xy);
            var result = z.CreateMediaItemDbContext();
            result.EnsureCreated().GetAwaiter().GetResult();
            return result;
        }

        internal class InMemoryEagleEyeDbContextFactory : EagleEyeDbContextFactory
        {
            public InMemoryEagleEyeDbContextFactory([CallerMemberName] string name = "dummy")
                : base(new DbContextOptionsBuilder<EagleEyeDbContext>()
                       .UseInMemoryDatabase(name)
//                       .UseSqlite("Data Source=:memory:;")
                       .Options)
            {
            }
        }
    }
}
