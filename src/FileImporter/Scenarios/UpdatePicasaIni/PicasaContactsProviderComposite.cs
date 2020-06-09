namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;
    using System.Linq;

    using EagleEye.Picasa.Picasa;

    public class PicasaContactsProviderComposite : IPicasaContactsProvider
    {
        private readonly IEnumerable<IPicasaContactsProvider> providers;

        public PicasaContactsProviderComposite(IEnumerable<IPicasaContactsProvider> providers)
        {
            this.providers = providers.ToArray();
        }

        public IEnumerable<PicasaPerson> GetPicasaContacts()
        {
            return providers.SelectMany(x => x.GetPicasaContacts()).Distinct();
        }
    }
}
