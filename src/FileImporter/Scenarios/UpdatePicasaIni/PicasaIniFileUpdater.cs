namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Linq;

    using EagleEye.Picasa.Picasa;

    public class PicasaIniFileUpdater
    {
        public PicasaIniFileUpdater(PicasaIniFile originalIniFile)
        {
            IniFile = new PicasaIniFile(originalIniFile);
        }

        public PicasaIniFile IniFile { get; }

        public void UpdateNameForId(string id, string name)
        {
            var newContact = new PicasaPerson(id, name);
            UpdateContactForId(id, newContact);
        }

        public void ReplaceContact(string id, PicasaPerson newContact)
        {
            var updated = false;
            foreach (var file in IniFile.Files)
            {
                foreach (var personWithLocation in file.Persons.Where(personWithLocation => personWithLocation.Person.Id == id))
                {
                    personWithLocation.UpdatePerson(newContact);
                    updated = true;
                }
            }

            foreach (var p in IniFile.Persons.Where(p => p.Id == id).ToArray())
                IniFile.Persons.Remove(p);

            if (updated)
                IniFile.Persons.Add(newContact);
        }

        public void UpdateContactForId(string id, PicasaPerson newContact)
        {
            foreach (var file in IniFile.Files)
            {
                foreach (var personWithLocation in file.Persons.Where(personWithLocation => personWithLocation.Person.Id == id))
                {
                    personWithLocation.UpdatePerson(newContact);
                }
            }

            foreach (var p in IniFile.Persons.Where(p => p.Id == id).ToArray())
                IniFile.Persons.Remove(p);

            IniFile.Persons.Add(newContact);
        }

        public void TagContactInPhoto(string filename, PicasaPersonLocation contact)
        {
            var file = IniFile.Files.SingleOrDefault(x => x.Filename == filename);
            if (file == null)
                return;

            file.Persons.Add((PicasaPersonLocation)contact.Clone());

            foreach (var p in IniFile.Persons.Where(p => p.Id == contact.Person.Id).ToArray())
                IniFile.Persons.Remove(p);

            IniFile.Persons.Add(contact.Person);
        }
    }
}
