namespace EagleEye.Core.Test.ReadModel
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
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
            var item1 = Create(1, DateTimeOffset.UtcNow); // should match predicate
            var item2 = Create(2, DateTimeOffset.UtcNow); // should match predicate
            var item3 = Create(3, DateTimeOffset.UtcNow.AddDays(1)); // should NOT match predicate

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

        private static Photo Create(int version, DateTimeOffset timestamp)
        {
            return new Photo
            {
                Id = Guid.NewGuid(),
                Version = version,
                EventTimestamp = timestamp,
            };
        }

        internal class InMemoryEagleEyeDbContextFactory : EagleEyeDbContextFactory
        {
            public InMemoryEagleEyeDbContextFactory([CallerMemberName] string name = "dummy")
                : base(new DbContextOptionsBuilder<EagleEyeDbContext>()
                       .UseInMemoryDatabase(name)
                       .Options)
            {
            }
        }
    }
}
