namespace EagleEye.ExifToolWrapper.MediaInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Interfaces;

    public class ExifToolGpsProvider : IMediaInformationProvider
    {
        private readonly IExifTool _exiftool;

        public ExifToolGpsProvider(IExifTool exiftool)
        {
            _exiftool = exiftool;
        }

        public int Priority { get; } = 100;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await _exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            // check for gps data.

            media.Location.SetCoordinates(1, 2);
        }
    }
}