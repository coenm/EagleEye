namespace EagleEye.FileImporter.Scenarios.MergePicasaContactsXml
{
    using EagleEye.Picasa.Picasa;

    public static class ContactNameMapping
    {
        public static PicasaContact? RenameContact(in PicasaContact contact)
        {
            var name = RenameName(contact.Name);
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return new PicasaContact(contact.Id, name, contact.DisplayName, contact.ModifiedDateTime, contact.IsLocalContact);
        }

        public static PicasaPerson? RenameContact(in PicasaPerson contact)
        {
            var name = RenameName(contact.Name);
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return new PicasaPerson(contact.Id, name);
        }

        private static string RenameName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return name.Trim();
        }
    }
}
