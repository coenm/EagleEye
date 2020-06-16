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
    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;

    using Lucene.Net.Store;

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

                foreach (var contact in contacts)
                {
                    if (resultingContacts.Contains(contact))
                        continue;

                    var sameId = resultingContacts.Where(x => x.Id == contact.Id).ToList();
                    if (sameId.Count == 0)
                        resultingContacts.Add(contact);
                    else
                        resultingContacts.Add(contact);
                }
            }

            if (File.Exists(destinationFilename))
                File.Delete(destinationFilename);
            File.Create(destinationFilename).Close();
            await using var stream = fileService.OpenWrite(destinationFilename);
            this.picasaContactsXmlFileWriter.Write(resultingContacts, stream);
        }
    }
}
