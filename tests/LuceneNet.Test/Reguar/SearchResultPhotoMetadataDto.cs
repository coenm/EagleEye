namespace LuceneNet.Test.Reguar
{
    public class SearchResultPhotoMetadataDto : PhotoMetadataDto
    {
        public SearchResultPhotoMetadataDto(string filename, params string[] persons) : base(filename, persons)
        {
        }

        public float Score { get; set; }
    }
}