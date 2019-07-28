namespace EagleEye.Photo.ReadModel.Similarity.Interface.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    [PublicAPI]
    public class SimilarityResultSet
    {
        internal SimilarityResultSet(Guid photoGuid, DateTimeOffset queryTimestamp, params SimilarityResult[] results)
        {
            PhotoGuid = photoGuid;
            QueryTimestamp = queryTimestamp;
            Matches = results?.ToList() ?? new List<SimilarityResult>();
        }

        public Guid PhotoGuid { get; }

        public DateTimeOffset QueryTimestamp { get; }

        [NotNull]
        public List<SimilarityResult> Matches { get; }
    }
}
