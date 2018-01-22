using System.Collections.Generic;
using FileImporter.Indexing;
using FileImporter.Infrastructure.ContentResolver;
using FileImporter.Infrastructure.FileIndexRepository;
using FileImporter.Infrastructure.JsonSimilarity;
using FileImporter.Infrastructure.PersistantSerializer;
using SimpleInjector;

namespace FileImporter
{
    using Similarity;

    public static class Startup
    {
        public static void ConfigureContainer(Container container, string indexFilename)
        {
            var similarityFilename = indexFilename + ".similarity.json";
            // todo check arguments.
            //            container.RegisterSingleton<IContentResolver>(new RelativeFilesystemContentResolver(rootPath));
            container.RegisterSingleton<IContentResolver>(FilesystemContentResolver.Instance);
            container.RegisterSingleton<IImageDataRepository>(() => new SingleImageDataRepository(new JsonToFileSerializer<List<ImageData>>(indexFilename)));
            container.RegisterSingleton<ISimilarityRepository>(() => new SingleFileSimilarityRepository(new JsonToFileSerializer<List<SimilarityResultStorage>>(similarityFilename)));

            container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}