namespace EagleEye.Core.Test.ReadModel
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class ExploringEntityFrameworkTests
    {
        [Fact, Xunit.Categories.Exploratory]
        public async Task SelectUsingPredicateFromEmptyDatabaseShouldReturnNothingTest()
        {
            // arrange
            var sut = new InMemoryMediaItemDbContextFactory();

            using (var db = sut.CreateMediaItemDbContext())
            {
                // act
                var result = await db.MediaItems
                                     .Where(item => item.TimeStampUtc <= DateTimeOffset.UtcNow)
                                     .ToListAsync()
                                     .ConfigureAwait(false);

                // assert
                result.Should().BeEmpty();
            }
        }

        private static MediaItemDb Create(int version, DateTimeOffset timestamp)
        {
            return new MediaItemDb
                       {
                           Id = Guid.NewGuid(),
                           Version = version,
                           TimeStampUtc = timestamp
                       };
        }

        [Fact, Xunit.Categories.Exploratory]
        public async Task SelectUsingPredicateShouldReturnAskedItemTest()
        {
            // arrange
            var item1 = Create(1, DateTimeOffset.UtcNow); // should match predicate
            var item2 = Create(2, DateTimeOffset.UtcNow); // should match predicate
            var item3 = Create(3, DateTimeOffset.UtcNow.AddDays(1)); // should NOT match predicate

            var sut = new InMemoryMediaItemDbContextFactory();

            using (var db = sut.CreateMediaItemDbContext())
            {
                await db.MediaItems.AddRangeAsync(item1, item2, item3).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);

                // act
                var result = await db.MediaItems
                                     .Where(item => item.TimeStampUtc <= DateTimeOffset.UtcNow)
                                     .ToListAsync()
                                     .ConfigureAwait(false);

                // assert
                result.Should().BeEquivalentTo(new[] { item1, item2 }.AsEnumerable());
            }
        }

        internal class InMemoryMediaItemDbContextFactory : MediaItemDbContextFactory
        {
            public InMemoryMediaItemDbContextFactory([CallerMemberName] string name = "dummy")
                : base(new DbContextOptionsBuilder<MediaItemDbContext>()
                       .UseInMemoryDatabase(name)
                       .Options)
            {
            }
        }
    }
}