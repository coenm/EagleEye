using System.Collections.Generic;
using FileImporter.Indexing;
using FileImporter.Infrastructure.ContentResolver;
using FileImporter.Infrastructure.FileIndexRepository;
using FileImporter.Infrastructure.PersistantSerializer;
using SimpleInjector;

namespace FileImporter
{
    public static class Startup
    {
        public static void ConfigureContainer(Container container, string rootPath, string indexFilename)
        {
            // todo check arguments.

//            container.RegisterSingleton<IContentResolver>(new RelativeFilesystemContentResolver(rootPath));
            container.RegisterSingleton<IContentResolver>(FilesystemContentResolver.Instance);
            container.RegisterSingleton<IFileIndexRepository>(() => new SingleFileIndexRepository(new JsonToFileSerializer<List<FileIndex>>(indexFilename)));

            container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}