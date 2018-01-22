using System;
using System.Linq;
using System.Threading.Tasks;
using FileImporter.Indexing;

namespace FileImporter.Similarity
{
    public class FilenameProgressData
    {
        public FilenameProgressData(int current, int total, string filename)
        {
            Current = current;
            Total = total;
            Filename = filename;
        }

        public string Filename { get; }

        public int Current { get; }

        public int Total { get; }
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

            progress.Report(new FilenameProgressData(0, allKnownImages.Length, ""));
            int index = 0;

            _similarityRepository.AutoSave(false);

            Parallel.ForEach(allKnownImages, i =>
            {
                progress.Report(new FilenameProgressData(index, allKnownImages.Length, i.Identifier));

                var similarityResult = new SimilarityResult
                {
                    OtherImageHash = i.Hashes.ImageHash,
                    AverageHash = CoenM.ImageSharp.CompareHash.Similarity(image.Hashes.AverageHash, i.Hashes.AverageHash),
                    DifferenceHash = CoenM.ImageSharp.CompareHash.Similarity(image.Hashes.DifferenceHash, i.Hashes.DifferenceHash),
                    PerceptualHash = CoenM.ImageSharp.CompareHash.Similarity(image.Hashes.PerceptualHash, i.Hashes.PerceptualHash)
                };

                _similarityRepository.AddOrUpdate(image.Hashes.ImageHash, similarityResult);
            });


            _similarityRepository.SaveChanges();

            _similarityRepository.AutoSave(true);
        }
    }
}