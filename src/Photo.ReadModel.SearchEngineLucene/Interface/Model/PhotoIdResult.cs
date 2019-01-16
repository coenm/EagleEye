namespace Photo.ReadModel.SearchEngineLucene.Interface.Model
{
    using System;

    public class PhotoIdResult
    {
        internal PhotoIdResult(Guid id, float score)
        {
            Id = id;
            Score = score;
        }

        public float Score { get; }

        public Guid Id { get; }
    }
}
