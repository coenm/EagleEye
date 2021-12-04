namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class UpdatePicasaIniFileExecutor
    {
        [NotNull] private readonly IFileService fileService;
        [NotNull] private readonly IPicasaContactsProvider picasaContactsProvider;
        [NotNull] private readonly IPicasaIniFileProvider picasaIniFileProvider;
        [NotNull] private readonly IPicasaIniFileWriter picasaIniFileWriter;

        public UpdatePicasaIniFileExecutor(
            [NotNull] IFileService fileService,
            [NotNull] IPicasaContactsProvider picasaContactsProvider,
            [NotNull] IPicasaIniFileProvider picasaIniFileProvider,
            [NotNull] IPicasaIniFileWriter picasaIniFileWriter)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            Guard.Argument(picasaContactsProvider, nameof(picasaContactsProvider)).NotNull();
            Guard.Argument(picasaIniFileProvider, nameof(picasaIniFileProvider)).NotNull();
            Guard.Argument(picasaIniFileWriter, nameof(picasaIniFileWriter)).NotNull();
            this.fileService = fileService;
            this.picasaContactsProvider = picasaContactsProvider;
            this.picasaIniFileProvider = picasaIniFileProvider;
            this.picasaIniFileWriter = picasaIniFileWriter;
        }

        public async Task HandleAsync([NotNull] string filename, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();

            // todo remove
            await Task.Yield();

            var currentConfig = picasaIniFileProvider.Get(filename);
            if (currentConfig == null)
                return;

            var updater = new PicasaIniFileUpdater(currentConfig);
            var backups = picasaIniFileProvider.GetBackups(filename).ToList();
            var contacts = picasaContactsProvider.GetPicasaContacts().ToArray();

            foreach (var file in currentConfig.Files)
            {
                var contactRegions = file.Persons.Where(p => p.Region.HasValue).Select(p => p.Region.Value).ToArray();

                var backuppedFiles = backups.SelectMany(backup => backup.Files)
                                           .Where(f => f.Filename == file.Filename
                                                       &&
                                                       f.Persons.Any(p => p.Region.HasValue
                                                                          &&
                                                                          !contactRegions.Contains(p.Region.Value)))
                                           .ToList();

                foreach (var backup in backuppedFiles)
                {
                    foreach (var contact in backup.Persons.Where(c => c.Region.HasValue && !contactRegions.Contains(c.Region.Value)))
                    {
                        // add contact
                        updater.TagContactInPhoto(file.Filename, contact);
                    }
                }

                var updatedFile = updater.IniFile.Files.First(f => f.Filename == file.Filename);
                foreach (var personWithLocation in updatedFile.Persons.ToList())
                {
                    var id = personWithLocation.Person.Id;
                    var region = personWithLocation.Region;

                    if (region.HasValue)
                    {
                        // check if backups has item with same coordinates.
                        var foundPersons = backups
                                           .SelectMany(picasaIniFile => picasaIniFile.Files)
                                           .Where(fileWithPersons => fileWithPersons.Filename == file.Filename)
                                           .SelectMany(fileWithPersons => fileWithPersons.Persons)
                                           .Where(personLocation => region.Equals(personLocation.Region))
                                           .Where(x => !string.IsNullOrWhiteSpace(x.Person.Name))
                                           .ToArray();

                        if (foundPersons.Length >= 1)
                            updater.UpdateNameForId(id, foundPersons[0].Person.Name);
                    }

                    // try get name
                    var selectedContacts = contacts.Where(c => c.Id == id).ToList();
                    if (selectedContacts.Count > 0)
                        updater.UpdateNameForId(id, selectedContacts[0].Name);
                }
            }

            if (updater.IniFile.Equals(currentConfig))
                return;

            // Do the update
            picasaIniFileWriter.Write(filename, updater, currentConfig, true);
        }

        public async Task ReplaceNames([NotNull] string filename, [NotNull] string mappingFile, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
            Guard.Argument(mappingFile, nameof(mappingFile)).NotNull();

            // todo remove
            await Task.Yield();

            var currentConfig = picasaIniFileProvider.Get(filename);
            if (currentConfig == null)
                return;

            var updater = new PicasaIniFileUpdater(currentConfig);
            var contacts = picasaContactsProvider.GetPicasaContacts().ToArray();

            var mapping = new Dictionary<string, string>();
            await using (var s = fileService.OpenRead(mappingFile))
            using (var r = new StreamReader(s))
            {
                var content = await r.ReadToEndAsync();

                var lines = content.Replace("\r", string.Empty).Split('\n');
                foreach (var line in lines)
                {
                    var x = line.Split(" ");
                    if (x.Length == 2)
                    {
                        mapping.Add(x[0], x[1]);
                    }
                }
            }

            foreach (var (old, replacement) in mapping)
            {
                var replacementContacts = contacts.Where(x => x.Id == replacement).ToArray();
                if (replacementContacts.Length == 0)
                    continue;
                if (replacementContacts.Length > 1)
                    throw new Exception("Too much contacts");

                updater.ReplaceContact(old, replacementContacts.Single());
            }

            if (updater.IniFile.Equals(currentConfig))
                return;

            // Do the update
            picasaIniFileWriter.Write(filename, updater, currentConfig);
        }
    }
}
