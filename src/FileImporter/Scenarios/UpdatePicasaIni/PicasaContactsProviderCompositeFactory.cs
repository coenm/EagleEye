namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using JetBrains.Annotations;

    public class PicasaContactsProviderCompositeFactory
    {
        [NotNull] private readonly IFileService fileService;
        [NotNull] private readonly IDirectoryService directoryService;

        public PicasaContactsProviderCompositeFactory([NotNull] IFileService fileService, [NotNull] IDirectoryService directoryService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            Guard.Argument(directoryService, nameof(directoryService)).NotNull();
            this.fileService = fileService;
            this.directoryService = directoryService;
        }

        public PicasaContactsProviderComposite Create(string picasaXmlContactsFilename, string iniFilesPath)
        {
            Guard.Argument(picasaXmlContactsFilename, nameof(picasaXmlContactsFilename)).NotNull().NotWhiteSpace();
            Guard.Argument(iniFilesPath, nameof(iniFilesPath)).NotNull().NotWhiteSpace();

            var providers = new List<IPicasaContactsProvider>
                            {
                                new XmlPicasaContactsProviderAdapter(fileService, picasaXmlContactsFilename),
                            };

            var picasaFileNames = new[] { ".picasa.ini", "Picasa.ini" };

            foreach (var file in directoryService.EnumerateFiles(iniFilesPath, "*.ini", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (picasaFileNames.Contains(fi.Name))
                        providers.Add(new IniPicasaContactsProviderAdapter(fileService, file));
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            return new PicasaContactsProviderComposite(providers);
        }
    }
}
