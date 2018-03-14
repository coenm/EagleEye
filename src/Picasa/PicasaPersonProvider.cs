namespace EagleEye.Picasa
{
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Interfaces;

    public class PicasaPersonProvider : IMediaInformationProvider
    {
        private readonly IPicasaService _picasaService;

        public PicasaPersonProvider(IPicasaService picasaService)
        {
            _picasaService = picasaService;
        }

        public int Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return _picasaService.CanProvideData(filename);
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await _picasaService.GetDataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return;

            foreach (var person in result.Persons)
                media.AddPerson(person);
        }
    }
}