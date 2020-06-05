namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class UpdatePicasaIniFileExecutor
    {
        [NotNull] private readonly IPicasaContactsProvider picasaContactsProvider;
        [NotNull] private readonly IPicasaIniFileProvider picasaIniFileProvider;
        [NotNull] private readonly IPicaseIniFileWriter picasaIniFileWriter;

        public UpdatePicasaIniFileExecutor(
            [NotNull] IPicasaContactsProvider picasaContactsProvider,
            [NotNull] IPicasaIniFileProvider picasaIniFileProvider,
            [NotNull] IPicaseIniFileWriter picasaIniFileWriter)
        {
            Guard.Argument(picasaContactsProvider, nameof(picasaContactsProvider)).NotNull();
            Guard.Argument(picasaIniFileProvider, nameof(picasaIniFileProvider)).NotNull();
            Guard.Argument(picasaIniFileWriter, nameof(picasaIniFileWriter)).NotNull();
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

            // var backups = picasaIniFileProvider.GetBackups(filename);

            var contacts = picasaContactsProvider.GetPicasaContacts().ToArray();

            foreach (var file in currentConfig.Files)
            {
                var updatedFile = updater.IniFile.Files.First(f => f.Filename == file.Filename);
                foreach (var personWithLocation in updatedFile.Persons.ToList())
                {
                    if (string.IsNullOrWhiteSpace(personWithLocation.Person.Name))
                    {
                        var id = personWithLocation.Person.Id;

                        // try get name
                        var selectedContacts = contacts.Where(c => c.Id == id).ToList();
                        if (selectedContacts.Any())
                            updater.UpdateNameForId(id, selectedContacts.First().Name);
                    }
                    else
                    {
                        // todo
                    }
                }
            }

            if (updater.IniFile.Equals(currentConfig))
                return;

            // Do the update
            picasaIniFileWriter.Write(filename, updater, currentConfig);
        }
    }
}
