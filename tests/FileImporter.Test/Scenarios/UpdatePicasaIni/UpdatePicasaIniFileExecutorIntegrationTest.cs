namespace EagleEye.FileImporter.Test.Scenarios.UpdatePicasaIni
{
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.FileImporter.Scenarios.UpdatePicasaIni;
    using VerifyXunit;
    using Xunit;
    using Xunit.Abstractions;

    public class UpdatePicasaIniFileExecutorIntegrationTest : VerifyBase
    {
        private const string DummyFilename = "M:\\Fotoalbum\\Coen\\2003-Coen\\.picasa.ini";
        private readonly IPicasaContactsProvider picasaContactsProvider;
        private readonly IPicasaIniFileProvider picasaIniFileProvider;

        private readonly UpdatePicasaIniFileExecutor sut;
        private readonly IPicasaIniFileWriter picasaIniFileWriter;

        public UpdatePicasaIniFileExecutorIntegrationTest(ITestOutputHelper output)
            : base(output)
        {
            var fileService = SystemFileService.Instance;
            var xmlFile = "M:\\FotoalbumBackups\\contacts.xml";
            var backupsDir = "M:\\FotoalbumBackups";
            var origDir = "M:\\Fotoalbum";

            var factory = new PicasaContactsProviderCompositeFactory(fileService, SystemDirectoryService.Instance);

            picasaContactsProvider = factory.Create(xmlFile, backupsDir);
            picasaIniFileProvider = new PicasaIniFileProvider(fileService, origDir, backupsDir);
            picasaIniFileWriter = new PicasaIniWriter(fileService);

            sut = new UpdatePicasaIniFileExecutor(picasaContactsProvider, picasaIniFileProvider, picasaIniFileWriter);
        }

        [Fact(Skip="tmp")]
        public async Task ExecuteAsync_ShouldNotGetContacts_WhenPicasaIniIsNull()
        {
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);
        }
    }
}
