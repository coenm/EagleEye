namespace Photo.ReadModel.Similarity.Test.Internal.EntityFramework
{
    using System;
    using System.Diagnostics;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using FluentAssertions;
    using Photo.ReadModel.Similarity.Test.Mocks;
    using Xunit;

    public class InternalSimilarityRepositoryTest
    {
        private readonly InternalSimilarityRepository sut;
        private readonly InMemorySimilarityDbContextFactory ctxFactory;

        private readonly HashIdentifiers hashIdentifier1;
        private readonly HashIdentifiers hashIdentifier2;
        private readonly HashIdentifiers hashIdentifier3;
        private readonly PhotoHash photoHash11;
        private readonly PhotoHash photoHash12;
        private readonly PhotoHash photoHash21;

        public InternalSimilarityRepositoryTest()
        {
            sut = new InternalSimilarityRepository();
            ctxFactory = new InMemorySimilarityDbContextFactory();
            ctxFactory.Initialize().GetAwaiter().GetResult();

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            using (var ctx = ctxFactory.CreateDbContext())
            {
                hashIdentifier1 = CreateHashIdentifiers(1, "aa");
                hashIdentifier2 = CreateHashIdentifiers(2, "bb");
                hashIdentifier3 = CreateHashIdentifiers(3, "cc");

                ctx.HashIdentifiers.AddRange(hashIdentifier1, hashIdentifier2, hashIdentifier3);

                photoHash11 = CreatePhotoHash(guid1, hashIdentifier1, 1, 2);
                photoHash12 = CreatePhotoHash(guid2, hashIdentifier1, 2, 4);
                photoHash21 = CreatePhotoHash(guid1, hashIdentifier2, 3, 6);

                ctx.PhotoHashes.AddRange(photoHash11, photoHash12, photoHash21);
                ctx.SaveChanges();
            }
        }

        [Fact]
        public void GetPhotoHashesByHashIdentifier_ShouldReturnCorrectItemsWithoutHashIdentifier()
        {
            using (var ctx = ctxFactory.CreateDbContext())
            {
                // arrange

                // act
                var result = sut.GetPhotoHashesByHashIdentifier(ctx, hashIdentifier1);

                // assert
                result.Should().BeEquivalentTo(
                    new[] { photoHash11, photoHash12 },
                    config => config.Excluding(hash =>
                        hash.HashIdentifier)); // HashIdentifier is not included in the query
            }
        }

        [DebuggerStepThrough]
        private static HashIdentifiers CreateHashIdentifiers(int id, string hashIdentifier)
        {
            return new HashIdentifiers
            {
                Id = id,
                HashIdentifier = hashIdentifier,
            };
        }

        [DebuggerStepThrough]
        private static PhotoHash CreatePhotoHash(Guid guid, HashIdentifiers hashIdentifier, ulong hash, int version)
        {
            return new PhotoHash
            {
                Id = guid,
                HashIdentifier = hashIdentifier,
                Hash = hash,
                HashIdentifiersId = hashIdentifier.Id,
                Version = version,
            };
        }
    }
}
