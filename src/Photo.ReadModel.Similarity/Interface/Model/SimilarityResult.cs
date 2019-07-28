namespace EagleEye.Photo.ReadModel.Similarity.Interface.Model
{
    using System;

    using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class SimilarityResult
    {
        internal SimilarityResult(Guid photoGuid, [NotNull] double score)
        {
            Guard.Argument(score, nameof(score)).InRange(0, 100);

            PhotoGuid = photoGuid;
            Score = score;
        }

        public Guid PhotoGuid { get; }

        public double Score { get; }
    }
}
