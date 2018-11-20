namespace EagleEye.Core.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.Model;
    using Helpers.Guards;
    using JetBrains.Annotations;

    internal class ReadModel : IReadModelFacade
    {
        [NotNull] private readonly IEagleEyeRepository repository;

        public ReadModel([NotNull] IEagleEyeRepository repository)
        {
            Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
        }

        public async Task<IEnumerable<Photo>> GetAllPhotosAsync()
        {
            var result = await repository.GetAllAsync().ConfigureAwait(false);

            return result?.Select(MapPhoto);
        }

        public async Task<Model.Photo> GetPhotoByGuidAsync(Guid id)
        {
            var result = await repository.GetByIdAsync(id).ConfigureAwait(false);

            if (result == null)
                return null;

            return MapPhoto(result);
        }

        private Photo MapPhoto([NotNull] EntityFramework.Models.Photo photo)
        {
            DebugGuard.NotNull(photo, nameof(photo));

            return new Photo(
                photo.Filename,
                photo.FileSha256,
                photo.Tags?.Select(x => x.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly(),
                photo.People?.Select(x => x.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly(),
                MapLocation(photo.Location),
                photo.Version);
        }

        [CanBeNull]
        private Location MapLocation([CanBeNull] EntityFramework.Models.Location location)
        {
            if (location == null)
                return null;

            return new Location(
                location.CountryCode,
                location.CountryName,
                location.City,
                location.State,
                location.SubLocation);
        }
    }
}
