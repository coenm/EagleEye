namespace EagleEye.Photo.ReadModel.Similarity.Interface.Model
{
    using System;

    using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class SimilarityResult
    {
        internal SimilarityResult(Guid photoGuid, [NotNull] string hashAlgorithm, float score, DateTimeOffset recordDate)
        {
            Guard.Argument(hashAlgorithm, nameof(hashAlgorithm)).NotNull().NotEmpty();
            Guard.Argument(score, nameof(score)).InRange(0, 100);

            PhotoGuid = photoGuid;
            HashAlgorithm = hashAlgorithm;
            Score = score;
            RecordDate = recordDate;
        }

        public Guid PhotoGuid { get; }

        public string HashAlgorithm { get; }

        public float Score { get; }

        public DateTimeOffset RecordDate { get; }
    }
}
