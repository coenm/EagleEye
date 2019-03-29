namespace EagleEye.FileImporter.Indexing
{
    using System.Collections.Generic;

    using EagleEye.FileImporter.Imaging;
    using Dawn;

    public class CalculateIndexService
    {
        private readonly IContentResolver contentResolver;

        public CalculateIndexService(IContentResolver contentResolver)
        {
            Guard.Argument(contentResolver, nameof(contentResolver)).NotNull();
            this.contentResolver = contentResolver;
        }

        public IEnumerable<ImageData> CalculateIndex(IReadOnlyList<string> fileIdentifiers)
        {
            Guard.Argument(fileIdentifiers, nameof(fileIdentifiers)).NotNull();

            var result = new ImageData[fileIdentifiers.Count];

            for (var index = 0; index < fileIdentifiers.Count; index++)
            {
                using (var stream = contentResolver.Read(fileIdentifiers[index]))
                {
                    result[index] = new ImageData(fileIdentifiers[index])
                    {
                        Hashes = ImageHashing.Calculate(stream),
                    };
                }
            }

            return result;
        }
    }
}
