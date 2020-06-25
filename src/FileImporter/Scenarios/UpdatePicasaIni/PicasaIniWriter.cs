namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;
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

        public void Write(string filename, PicasaIniFileUpdater updated, PicasaIniFile original, bool onlyContacts = false)
        {
            var originalIniFile = new IniFile();

            using (var stream = fileService.OpenRead(filename))
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                originalIniFile.Load(sr);
            }

            var newIniFile = new IniFile();

            foreach (var sectionName in originalIniFile.GetSections())
            {
                var items = originalIniFile.GetSectionSettings(sectionName);

                if (sectionName.Equals(ContactsKey))
                {
                    WriteContacts(updated, items, newIniFile, sectionName);
                    continue;
                }

                if (onlyContacts)
                {
                    foreach (IniSetting contactItem in items)
                    {
                        newIniFile.SetSetting(sectionName, contactItem.Name, contactItem.Value);
                    }

                    continue;
                }

                if (sectionName.Equals("Picasa"))
                {
                    foreach (IniSetting contactItem in items)
                    {
                        newIniFile.SetSetting(sectionName, contactItem.Name, contactItem.Value);
                    }

                    continue;
                }

                // files
                foreach (IniSetting item in items)
                {
                    if (item.Name != "faces")
                    {
                        newIniFile.SetSetting(sectionName, item.Name, item.Value);
                        continue;
                    }

                    var facesValue = item.Value;
                    var updatedFile = updated.IniFile.Files.SingleOrDefault(x => x.Filename == sectionName);
                    var originalFile = original.Files.SingleOrDefault(x => x.Filename == sectionName);
                    if (updatedFile != null)
                    {
                        var updatedPersons = updatedFile.Persons.ToList();
                        var originalPersons = originalFile?.Persons.ToList() ?? new List<PicasaPersonLocation>(0);

                        if (!updatedPersons.SequenceEqual(originalPersons))
                        {
                            facesValue = string.Empty;

                            foreach (var person in originalPersons)
                            {
                                var originalPersonId = person.Person.Id;
                                if (!person.Region.HasValue)
                                    continue;

                                var originalPersonRect64 = person.Region.Value.Rect64;

                                var matchingPersons = updatedPersons.Where(x => x.Equals(person)).ToArray();

                                var matchingPerson = matchingPersons.FirstOrDefault();
                                if (matchingPerson != null)
                                {
                                    facesValue += originalPersonRect64 + "," + originalPersonId + ";";
                                    updatedPersons.Remove(matchingPerson);
                                }
                                else
                                {
                                    var matchingPersonsOnRegion = updatedPersons.FirstOrDefault(x => x.Region.Equals(person.Region));
                                    if (matchingPersonsOnRegion?.Region != null)
                                    {
                                        facesValue += matchingPersonsOnRegion.Region.Value.Rect64 + "," + matchingPersonsOnRegion.Person.Id + ";";
                                        updatedPersons.Remove(matchingPersonsOnRegion);
                                    }
                                }
                            }

                            foreach (var person in updatedPersons.Where(p => p.Region.HasValue))
                            {
                                facesValue += person.Region.Value.Rect64 + "," + person.Person.Id + ";";
                            }

                            facesValue = facesValue.Substring(0, facesValue.Length - 1);
                        }
                    }

                    newIniFile.SetSetting(sectionName, item.Name, facesValue);
                }
            }

            using (var fileStream = fileService.OpenWrite(filename))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                newIniFile.Save(writer);
                writer.Flush();
            }

            return;
        }

        private static void WriteContacts(PicasaIniFileUpdater updated, IEnumerable<IniSetting> items, IniFile newIniFile, string sectionName)
        {
            var updatedContacts = updated.IniFile.Persons.ToList();
            foreach (IniSetting contactItem in items)
            {
                var picasaContactName = contactItem.Value + ";;";
                var picasaContactKey = contactItem.Name;
                var match = updatedContacts.Where(p => p.Id == picasaContactKey && p.Name == picasaContactName).ToArray();
                if (match.Length > 0)
                {
                    newIniFile.SetSetting(sectionName, picasaContactKey, picasaContactName);
                    updatedContacts.RemoveAll(p => match.Contains(p));
                    continue;
                }

                match = updatedContacts.Where(p => p.Id == picasaContactKey || p.Name == picasaContactName).ToArray();
                if (match.Length == 0)
                    continue;

                foreach (var matchedPerson in match)
                {
                    newIniFile.SetSetting(sectionName, matchedPerson.Id, matchedPerson.Name + ";;");
                }

                updatedContacts.RemoveAll(p => match.Contains(p));
            }

            foreach (var picasaPerson in updatedContacts)
            {
                newIniFile.SetSetting(sectionName, picasaPerson.Id, picasaPerson.Name + ";;");
            }
        }
    }
}
