namespace Photo.ReadModel.Similarity.Test.Internal.EntityFramework
{
    using System;
    using System.Diagnostics;

    using FluentAssertions;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Photo.ReadModel.Similarity.Test.Mocks;
    using Xunit;

    public class InternalSimilarityRepositoryTest
    {
        private readonly InternalSimilarityRepository sut;
        private readonly InMemorySimilarityDbContextFactory ctxFactory;

        private HashIdentifiers hashIdentifier1;
        private HashIdentifiers hashIdentifier2;
        private HashIdentifiers hashIdentifier3;
        private PhotoHash photoHash11;
        private PhotoHash photoHash12;
        private PhotoHash photoHash21;

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

                photoHash11 = CreatePhotoHash(guid1, hashIdentifier1, new byte[1], 2);
                photoHash12 = CreatePhotoHash(guid2, hashIdentifier1, new byte[2], 4);
                photoHash21 = CreatePhotoHash(guid1, hashIdentifier2, new byte[3], 6);

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
                    new[] {photoHash11, photoHash12},
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
        private static PhotoHash CreatePhotoHash(Guid guid, HashIdentifiers hashIdentifier, byte[] hash, int version)
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
