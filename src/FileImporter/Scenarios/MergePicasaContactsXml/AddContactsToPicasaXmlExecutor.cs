namespace EagleEye.FileImporter.Scenarios.MergePicasaContactsXml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.FileImporter.Scenarios.UpdatePicasaIni;
    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class AddContactsToPicasaXmlExecutor
    {
        private readonly PicasaContactsXmlReader picasaContactsReader;
        private readonly PicasaContactsXmlWriter picasaContactsXmlFileWriter;
        private readonly IFileService fileService;
        private readonly IPicasaContactsProvider picasaContactsProvider;
        private readonly IPicasaIniFileProvider picasaIniFileProvider;
        private readonly IDateTimeService dateTimeService;

        public AddContactsToPicasaXmlExecutor(
            [NotNull] PicasaContactsXmlReader picasaContactsReader,
            [NotNull] PicasaContactsXmlWriter picasaContactsXmlFileWriter,
            [NotNull] IFileService fileService,
            [NotNull] IPicasaContactsProvider picasaContactsProvider,
            [NotNull] IPicasaIniFileProvider picasaIniFileProvider,
            [NotNull] IDateTimeService dateTimeService)
        {
            Guard.Argument(picasaContactsReader, nameof(picasaContactsReader)).NotNull();
            Guard.Argument(picasaContactsXmlFileWriter, nameof(picasaContactsXmlFileWriter)).NotNull();
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            Guard.Argument(picasaContactsProvider, nameof(picasaContactsProvider)).NotNull();
            Guard.Argument(picasaIniFileProvider, nameof(picasaIniFileProvider)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();

            this.picasaContactsReader = picasaContactsReader;
            this.picasaContactsXmlFileWriter = picasaContactsXmlFileWriter;
            this.fileService = fileService;
            this.picasaContactsProvider = picasaContactsProvider;
            this.picasaIniFileProvider = picasaIniFileProvider;
            this.dateTimeService = dateTimeService;
        }

        public async Task HandleAsync([NotNull] string destinationFilename, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default)
        {
            Guard.Argument(destinationFilename, nameof(destinationFilename)).NotNull();

            // todo remove
            await Task.Yield();

            PicasaPerson[] allContacts = picasaContactsProvider.GetPicasaContacts().ToArray();
            List<PicasaContact> xmlContacts = picasaContactsReader.GetContactsFromFile(destinationFilename);
            List<PicasaContact> added = new List<PicasaContact>();

            var date = dateTimeService.Now;
            var dateString = date.ToString("yyyy-MM-ddTHH:mm:sszzz");

            foreach (var contact1 in allContacts)
            {
                var nullableContact = ContactNameMapping.RenameContact(contact1);
                if (!nullableContact.HasValue)
                    continue;

                var contact = nullableContact.Value;

                if (xmlContacts.Count(x => x.Id == contact.Id) > 0)
                    continue;

                var picasaContact = new PicasaContact(contact.Id, contact.Name, null, dateString, "1");
                xmlContacts.Add(picasaContact);
                added.Add(picasaContact);
            }

            if (File.Exists(destinationFilename))
                File.Delete(destinationFilename);
            File.Create(destinationFilename).Close();
            await using var stream = fileService.OpenWrite(destinationFilename);
            this.picasaContactsXmlFileWriter.Write(xmlContacts, stream);
        }
    }
}
