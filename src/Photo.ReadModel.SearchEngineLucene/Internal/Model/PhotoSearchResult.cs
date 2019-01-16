namespace Photo.ReadModel.SearchEngineLucene.Internal.Model
{
    internal class PhotoSearchResult : Photo
    {
        internal PhotoSearchResult(float score)
        {
            Score = score;
        }

        public float Score { get; set; }
    }
}
