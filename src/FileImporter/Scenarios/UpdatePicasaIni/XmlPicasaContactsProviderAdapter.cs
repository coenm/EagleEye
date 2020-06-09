namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;

    public class XmlPicasaContactsProviderAdapter : IPicasaContactsProvider
    {
        private readonly string fileName;
        private readonly PicasaContactsXmlReader reader;

        public XmlPicasaContactsProviderAdapter([NotNull] IFileService fileService, string fileName)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            Guard.Argument(fileName, nameof(fileName)).NotNull().NotWhiteSpace();
            reader = new PicasaContactsXmlReader(fileService);
            this.fileName = fileName;
        }

        public IEnumerable<PicasaPerson> GetPicasaContacts()
        {
            return reader.GetContactsFromFile(fileName);
        }
    }
}
