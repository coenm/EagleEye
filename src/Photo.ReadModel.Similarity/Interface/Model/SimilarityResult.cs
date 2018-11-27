namespace Photo.ReadModel.Similarity.Interface.Model
{
    using System;

    public class SimilarityResult
    {
        internal SimilarityResult(Guid id, float score, DateTimeOffset recordDate)
        {
            Id = id;
            Score = score;
            RecordDate = recordDate;
        }

        public Guid Id { get; }

        public float Score { get; }

        public DateTimeOffset RecordDate { get; }
    }
}
