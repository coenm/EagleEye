namespace EagleEye.Picasa
{
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Interfaces;
    using Helpers.Guards;
    using JetBrains.Annotations;

    public class PicasaPersonProvider : IMediaInformationProvider
    {
        private readonly IPicasaService picasaService;

        public PicasaPersonProvider([NotNull] IPicasaService picasaService)
        {
            Guard.NotNull(picasaService, nameof(picasaService));
            this.picasaService = picasaService;
        }

        public int Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return picasaService.CanProvideData(filename);
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await picasaService.GetDataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return;

            foreach (var person in result.Persons)
                media.AddPerson(person);
        }
    }
}
