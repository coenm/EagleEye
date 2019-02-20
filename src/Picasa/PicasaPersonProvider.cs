namespace EagleEye.Picasa
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;

    using Helpers.Guards;
    using JetBrains.Annotations;

    public class PicasaPersonProvider : IPhotoPersonProvider, IMediaInformationProvider
    {
        private readonly IPicasaService picasaService;

        public PicasaPersonProvider([NotNull] IPicasaService picasaService)
        {
            Guard.NotNull(picasaService, nameof(picasaService));
            this.picasaService = picasaService;
        }

        public string Name => nameof(PicasaPersonProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return picasaService.CanProvideData(filename);
        }

        public async Task<List<string>> ProvideAsync(string filename)
        {
            var result = await picasaService.GetDataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return new List<string>();

            return result.Persons.ToList();
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await ProvideAsync(filename).ConfigureAwait(false);

            foreach (var person in result)
                media.AddPerson(person);
        }
    }
}
