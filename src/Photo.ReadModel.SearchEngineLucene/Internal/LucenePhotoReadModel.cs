namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    using Dawn;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using JetBrains.Annotations;

    internal class LucenePhotoReadModel : IReadModel
    {
        [NotNull]
        private readonly IPhotoIndex photoIndex;

        public LucenePhotoReadModel(IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
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

            // todo skip, take
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
            Guard.Argument(photo, nameof(photo)).NotNull();

            // todo datetime taken.
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
            Guard.Argument(photo, nameof(photo)).NotNull();
            return new Interface.Model.PhotoIdResult(photo.Id, photo.Score);
        }
    }
}
