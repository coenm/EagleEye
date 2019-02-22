namespace EagleEye.Picasa
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;
    using JetBrains.Annotations;

    public class PicasaPersonProvider : IPhotoPersonProvider
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

        public async Task<List<string>> ProvideAsync(string filename, [CanBeNull] List<string> previousResult)
        {
            DebugGuard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            var persons = await picasaService.GetDataAsync(filename).ConfigureAwait(false);

            if (persons == null)
                return previousResult;

            if (previousResult == null)
                return null;

            previousResult.AddRange(persons.Persons);
            return previousResult;
        }
    }
}
