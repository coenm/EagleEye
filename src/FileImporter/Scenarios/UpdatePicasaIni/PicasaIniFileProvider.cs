namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;

    public class PicasaIniFileProvider : IPicasaIniFileProvider
    {
        private readonly IFileService fileService;
        private readonly string origDirectory;
        private readonly string backupDirectory;

        public PicasaIniFileProvider(IFileService fileService, string origDirectory, string backupDirectory)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();

            this.fileService = fileService;
            this.origDirectory = origDirectory;
            this.backupDirectory = backupDirectory;
        }

        public IEnumerable<PicasaIniFile> GetBackups(string originalFilename)
        {
            var newFilename = originalFilename.Replace(origDirectory, backupDirectory);
            if (fileService.FileExists(newFilename))
                yield return Get(newFilename);
        }

        public PicasaIniFile Get(string filename)
        {
            using var stream = fileService.OpenRead(filename);
            return PicasaIniParser.Parse(stream);
        }
    }
}
