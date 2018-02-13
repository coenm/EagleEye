using System.Collections.Generic;

namespace EagleEye.LuceneNet.Test.Reguar
{
    internal static class StaticDocuments
    {
        public static IEnumerable<PhotoMetadataDto> Photos
        {
            get
            {
                yield return new PhotoMetadataDto("file1", "Marco van Basten", "Ruud Gulit", "Leo Messi");
                yield return new PhotoMetadataDto("file2", "Marco van Basten", "Leo Messi");
                yield return new PhotoMetadataDto("file3", "Marco van Basten", "Leo Messi");
                yield return new PhotoMetadataDto("file4", "Mark van Bommel", "Ruud Gulit");
                yield return new PhotoMetadataDto("file5", "", "Ruud Gulit", "Robin van Persie");
                yield return new PhotoMetadataDto("file6", "Robin v. Persie", "Robin van Persie");
                yield return new PhotoMetadataDto("file7");
                yield return new PhotoMetadataDto("file8", "van Persie", "Arjen Robben");
            }
        }
    }
}