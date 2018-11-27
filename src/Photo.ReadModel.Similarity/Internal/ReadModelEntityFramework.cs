namespace Photo.ReadModel.Similarity.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Helpers.Guards;

    using JetBrains.Annotations;

    using Photo.ReadModel.Similarity.Interface;
    using Photo.ReadModel.Similarity.Interface.Model;

    internal class ReadModelEntityFramework : ISimilarityReadModel
    {
        // [NotNull] private readonly IEagleEyeRepository repository;

        public ReadModelEntityFramework(/*[NotNull] IEagleEyeRepository repository*/)
        {
            // Guard.NotNull(repository, nameof(repository));
            // this.repository = repository;
        }


        public Task<IEnumerable<SimilarityResult>> GetAllSimilaritiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAllDistinctPhotosAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAllSimilaritiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAllUsedHashIdentifiers()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SimilarityResult>> GetAllSimilaritiesAsync(Guid id, string hashIdentifier, float score)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAllSimilaritiesAsync(Guid id, string hashIdentifier, float score)
        {
            throw new NotImplementedException();
        }

        // public async Task<IEnumerable<Interface.Model.Photo>> GetAllPhotosAsync()
        // {
        //     var result = await repository.GetAllAsync().ConfigureAwait(false);
        //
        //     return result?.Select(MapPhoto).ToList();
        // }
        //
        // public async Task<Interface.Model.Photo> GetPhotoByGuidAsync(Guid id)
        // {
        //     var result = await repository.GetByIdAsync(id).ConfigureAwait(false);
        //
        //     if (result == null)
        //         return null;
        //
        //     return MapPhoto(result);
        // }
        //
        // private Interface.Model.Photo MapPhoto([NotNull] Photo photo)
        // {
        //     DebugGuard.NotNull(photo, nameof(photo));
        //
        //     return new Interface.Model.Photo(
        //         photo.Id,
        //         photo.Filename,
        //         photo.FileMimeType,
        //         photo.FileSha256,
        //         photo.Tags?.Select(x => x.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly(),
        //         photo.People?.Select(x => x.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly(),
        //         MapLocation(photo.Location),
        //         photo.DateTimeTaken,
        //         photo.Version);
        // }
        //
        // [CanBeNull]
        // private Interface.Model.Location MapLocation([CanBeNull] Location location)
        // {
        //     if (location == null)
        //         return null;
        //
        //     return new Interface.Model.Location(
        //         location.CountryCode,
        //         location.CountryName,
        //         location.City,
        //         location.State,
        //         location.SubLocation);
        // }
    }
}
