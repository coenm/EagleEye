namespace SearchEngine.LuceneNet.ReadModel.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using SearchEngine.LuceneNet.ReadModel.Interface;
    using SearchEngine.LuceneNet.ReadModel.Internal.LuceneNet;

    internal class LucenePhotoReadModel : IReadModel
    {
        [NotNull]
        private readonly PhotoIndex photoIndex;

        public LucenePhotoReadModel(PhotoIndex photoIndex)
        {
            Guard.NotNull(photoIndex, nameof(photoIndex));
            this.photoIndex = photoIndex;
        }

        public List<Interface.Model.PhotoResult> FullSearch(string query)
        {
            var result = photoIndex.Search(query, out _);
            // todo skip, take,
            return result.Select(MapToPhotoResult).ToList();
        }

        public List<Interface.Model.PhotoIdResult> Search(string query)
        {
            var result = photoIndex.Search(query, out _);
            // todo skip, take,
            return result.Select(MapToPhotoIdResult).ToList();
        }

        public int Count(string query)
        {
            _ = photoIndex.Search(query, out var count);
            return count;
        }

        [NotNull]
        private Interface.Model.PhotoResult MapToPhotoResult([NotNull] Model.PhotoSearchResult photo)
        {
            DebugGuard.NotNull(photo, nameof(photo));

            return new Interface.Model.PhotoResult(
                photo.Id,
                photo.FileName,
                photo.FileMimeType,
                photo.Tags.ToList().AsReadOnly(),
                photo.Persons.ToList().AsReadOnly(),
                Interface.Model.Location.Create(
                    photo.LocationCountryCode,
                    photo.LocationCountryName,
                    photo.LocationCity,
                    photo.LocationState,
                    photo.LocationSubLocation),
                photo.Version,
                photo.Score);
        }

        [NotNull]
        private Interface.Model.PhotoIdResult MapToPhotoIdResult([NotNull] Model.PhotoSearchResult photo)
        {
            DebugGuard.NotNull(photo, nameof(photo));
            return new Interface.Model.PhotoIdResult(photo.Id, photo.Score);
        }
    }
}
