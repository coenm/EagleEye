namespace EagleEye.Picasa.PhotoProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    internal class PicasaPersonProvider : IPhotoPersonProvider
    {
        private readonly IPicasaService picasaService;

        public PicasaPersonProvider([NotNull] IPicasaService picasaService)
        {
            Guard.Argument(picasaService, nameof(picasaService)).NotNull();
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

            return result?.Persons.Select(x => x.Person.Name).Distinct().ToList();
        }
    }
}
