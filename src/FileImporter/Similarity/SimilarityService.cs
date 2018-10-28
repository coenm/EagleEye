namespace EagleEye.FileImporter.Similarity
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Indexing;

    public class SimilarityService
    {
        private readonly ISimilarityRepository similarityRepository;
        private readonly IImageDataRepository imageRepository;

        public SimilarityService(ISimilarityRepository similarityRepository, IImageDataRepository imageRepository)
        {
            this.imageRepository = imageRepository;
            this.similarityRepository = similarityRepository;
        }

        public void Update(ImageData image, IProgress<FilenameProgressData> progress)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (progress == null)
                progress = new Progress<FilenameProgressData>(i => { });

            var allKnownImageHashes = similarityRepository.FindAllRecordedMatches(image.Hashes.ImageHash);
            var allKnownImages = imageRepository.Find(img => img.Identifier != image.Identifier && !allKnownImageHashes.Contains(img.Hashes.ImageHash)).ToArray();

            progress.Report(new FilenameProgressData(0, allKnownImages.Length, ""));
            int index = 0;

            similarityRepository.AutoSave(false);

            Parallel.ForEach(allKnownImages, i =>
            {
                progress.Report(new FilenameProgressData(index, allKnownImages.Length, i.Identifier));

                var similarityResult = new SimilarityResult
                {
                    OtherImageHash = i.Hashes.ImageHash,
                    AverageHash = CoenM.ImageHash.CompareHash.Similarity(image.Hashes.AverageHash, i.Hashes.AverageHash),
                    DifferenceHash = CoenM.ImageHash.CompareHash.Similarity(image.Hashes.DifferenceHash, i.Hashes.DifferenceHash),
                    PerceptualHash = CoenM.ImageHash.CompareHash.Similarity(image.Hashes.PerceptualHash, i.Hashes.PerceptualHash),
                };

                similarityRepository.AddOrUpdate(image.Hashes.ImageHash, similarityResult);
            });

            similarityRepository.SaveChanges();

            similarityRepository.AutoSave(true);
        }
    }
}
