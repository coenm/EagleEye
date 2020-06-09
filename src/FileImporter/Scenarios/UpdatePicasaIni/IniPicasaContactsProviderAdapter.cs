namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;
    using System.Linq;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;

    public class IniPicasaContactsProviderAdapter : IPicasaContactsProvider
    {
        private readonly IFileService fileService;
        private readonly string filename;

        public IniPicasaContactsProviderAdapter(IFileService fileService, string filename)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();
            this.fileService = fileService;
            this.filename = filename;
        }

        public IEnumerable<PicasaPerson> GetPicasaContacts()
        {
            using var stream = fileService.OpenRead(filename);
            var ini = PicasaIniParser.Parse(stream);
            return ini?.Persons.AsEnumerable() ?? Enumerable.Empty<PicasaPerson>();
        }
    }
}
