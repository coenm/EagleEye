using System;
using System.Linq;
using FileImporter.Indexing;

namespace FileImporter.Similarity
{
    public class FilenameProgressData
    {
        public FilenameProgressData(int current, int total)
        {
            
        }

        public string Filename { get; set; }

        public int Current { get; set; }
        public int Total { get; set; }
    }

    public class SimilarityService
    {
        private readonly ISimilarityRepository _similarityRepository;
        private readonly IImageDataRepository _imageRepository;

        public SimilarityService(ISimilarityRepository similarityRepository, IImageDataRepository imageRepository)
        {
            _imageRepository = imageRepository;
            _similarityRepository = similarityRepository;
        }

        public void Update(ImageData image, IProgress<FilenameProgressData> progress)
        {

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (progress == null)
                progress = new Progress<FilenameProgressData>(i => { });

            var allKnownImageHashes = _similarityRepository.FindAllRecordedMatches(image.Hashes.ImageHash);
            var allKnownImages = _imageRepository.Find(img => img.Identifier != image.Identifier && !allKnownImageHashes.Contains(img.Hashes.ImageHash)).ToArray();

            progress.Report(new FilenameProgressData(0, allKnownImages.Length));
            int index = 0;
            foreach (var i in allKnownImages)
            {
                progress.Report(new FilenameProgressData(index, allKnownImages.Length));

                _similarityRepository.AddOrUpdate(image.Hashes.ImageHash, new SimilarityResult
                    {
                        OtherImageHash = i.Hashes.ImageHash,
                        AverageHash = CoenM.ImageSharp.CompareHash.Similarity(image.Hashes.AverageHash, i.Hashes.AverageHash),
                        DifferenceHash = CoenM.ImageSharp.CompareHash.Similarity(image.Hashes.DifferenceHash, i.Hashes.DifferenceHash),
                        PerceptualHash = CoenM.ImageSharp.CompareHash.Similarity(image.Hashes.PerceptualHash, i.Hashes.PerceptualHash)

                    });

                index++;
            }
        }
    }
}