namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.IO;
    using System.Linq;
    using System.Text;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.Picasa;
    using SoftCircuits.IniFileParser;

    public class PicasaIniWriter : IPicasaIniFileWriter
    {
        private const string ContactsKey = "Contacts2";
        private readonly IFileService fileService;

        public PicasaIniWriter(IFileService fileService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.fileService = fileService;
        }

        public void Write(string filename, PicasaIniFileUpdater updated, PicasaIniFile original)
        {
            var iniFile = new IniFile();

            using (var stream = fileService.OpenRead(filename))
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                iniFile.Load(sr);
            }

            var contactsSection = iniFile.GetSectionSettings(ContactsKey);
            if (contactsSection != null)
            {
                if (!updated.IniFile.Persons.SequenceEqual(original.Persons))
                {
                    foreach (var p in updated.IniFile.Persons)
                    {
                        if (original.Persons.Contains(p))
                            continue;

                        iniFile.SetSetting(ContactsKey, p.Id, p.Name + ";;");
                    }
                }
            }

            // todo update each file.

            using (var fileStream = fileService.OpenWrite(filename))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                iniFile.Save(writer);
                writer.Flush();
            }

            return;
        }
    }
}
