namespace EagleEye.Core.Test.ReadModel
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class ExploringEntityFrameworkTests
    {
        [Fact, Xunit.Categories.Exploratory]
        public async Task SelectUsingPredicateTest()
        {
            // arrange
            var sut = new InMemoryMediaItemDbContextFactory();

            using (var db = sut.CreateMediaItemDbContext())
            {
                var result = await db.MediaItems
                                     .Where(item => item.TimeStampUtc <= DateTimeOffset.UtcNow)
                                     .ToListAsync()
                                     .ConfigureAwait(false);

                result.Should().BeEmpty();
            }
        }

        [Fact, Xunit.Categories.Exploratory]
        public async Task SelectUsingPredicateTesta()
        {
            var sut = new InMemoryMediaItemDbContextFactory();

            using (var db = sut.CreateMediaItemDbContext())
            {
                var items = new[]
                                {
                                    Create(1, DateTimeOffset.UtcNow),
                                    Create(1, DateTimeOffset.UtcNow),
                                };
                await db.MediaItems.AddRangeAsync(items).ConfigureAwait(false);

                // item does not match predicate
                await db.MediaItems.AddAsync(Create(1, DateTimeOffset.UtcNow.AddDays(1)))
                        .ConfigureAwait(false);

                await db.SaveChangesAsync().ConfigureAwait(false);

                var result = await db.MediaItems
                                     .Where(item => item.TimeStampUtc <= DateTimeOffset.UtcNow)
                                     .ToListAsync()
                                     .ConfigureAwait(false);

                result.Should().BeEquivalentTo(items.AsEnumerable());
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

        internal class InMemoryMediaItemDbContextFactory : MediaItemDbContextFactory
        {
            public InMemoryMediaItemDbContextFactory()
                : base(new DbContextOptionsBuilder<MediaItemDbContext>()
                       .UseInMemoryDatabase("blaBlieBlow")
                       .Options)
            {
            }
        }
    }
}