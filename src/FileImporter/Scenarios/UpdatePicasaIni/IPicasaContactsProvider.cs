namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;

    using EagleEye.Picasa.Picasa;

    public interface IPicasaContactsProvider
    {
        IEnumerable<PicasaPerson> GetPicasaContacts();
    }
}
