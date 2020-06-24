namespace EagleEye.FileImporter.Scenarios.MergePicasaContactsXml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class MergePicasaXmlExecutor
    {
        private readonly PicasaContactsXmlReader picasaContactsReader;
        private readonly PicasaContactsXmlWriter picasaContactsXmlFileWriter;
        private readonly IFileService fileService;

        public MergePicasaXmlExecutor(
            [NotNull] PicasaContactsXmlReader picasaContactsReader,
            [NotNull] PicasaContactsXmlWriter picasaContactsXmlFileWriter,
            [NotNull] IFileService fileService)
        {
            Guard.Argument(picasaContactsReader, nameof(picasaContactsReader)).NotNull();
            Guard.Argument(picasaContactsXmlFileWriter, nameof(picasaContactsXmlFileWriter)).NotNull();
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.picasaContactsReader = picasaContactsReader;
            this.picasaContactsXmlFileWriter = picasaContactsXmlFileWriter;
            this.fileService = fileService;
        }

        public async Task StripDuplicatesAsync([NotNull] string sourceFile, [NotNull] string destinationFilename, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default)
        {
            Guard.Argument(sourceFile, nameof(sourceFile)).NotNull();
            Guard.Argument(destinationFilename, nameof(destinationFilename)).NotNull();

            // todo remove
            await Task.Yield();

            var contacts = picasaContactsReader.GetContactsFromFile(sourceFile);

            var orderedItems = contacts.OrderBy(x => x.Name).ThenBy(x => x.DisplayName).ToList();

            var result = new List<PicasaContact>();

            var mapping = new List<string>();
            foreach (var item in orderedItems)
            {
                var match = result.SingleOrDefault(x => x.Name == item.Name);
                if (match.Id == default)
                    result.Add(item);
                else
                    mapping.Add($"{item.Id} {match.Id}");
            }

            var mappingFile = destinationFilename + ".mapping.txt";
            if (File.Exists(mappingFile))
                File.Delete(mappingFile);
            await File.WriteAllLinesAsync(mappingFile, mapping, Encoding.UTF8, ct).ConfigureAwait(false);

            if (File.Exists(destinationFilename))
                File.Delete(destinationFilename);
            File.Create(destinationFilename).Close();
            await using var stream = fileService.OpenWrite(destinationFilename);
            this.picasaContactsXmlFileWriter.Write(result, stream);
        }

        public async Task HandleAsync([NotNull] string destinationFilename, [NotNull] List<string> sourceFiles, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default)
        {
            Guard.Argument(destinationFilename, nameof(destinationFilename)).NotNull();
            Guard.Argument(sourceFiles, nameof(sourceFiles)).NotNull();

            // todo remove
            await Task.Yield();

            var sources = sourceFiles.ToList();

            var resultingContacts = new List<PicasaContact>();
            foreach (var s in sources)
            {
                var contacts = picasaContactsReader.GetContactsFromFile(s);

                foreach (var contact1 in contacts)
                {
                    var contact = ContactNameMapping.RenameContact(contact1);
                    if (contact.HasValue == false)
                        continue;

                    if (resultingContacts.Contains(contact.Value))
                        continue;

                    var sameId = resultingContacts.Where(x => x.Id == contact.Value.Id).ToList();
                    if (sameId.Count == 0)
                        resultingContacts.Add(contact.Value);
                }
            }

            if (File.Exists(destinationFilename))
                File.Delete(destinationFilename);
            File.Create(destinationFilename).Close();

            await using var stream = fileService.OpenWrite(destinationFilename);
            picasaContactsXmlFileWriter.Write(resultingContacts, stream);
        }
    }
}
