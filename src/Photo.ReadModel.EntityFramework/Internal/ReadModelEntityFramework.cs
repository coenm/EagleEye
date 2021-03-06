﻿namespace EagleEye.Photo.ReadModel.EntityFramework.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Photo.ReadModel.EntityFramework.Interface;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using JetBrains.Annotations;

    internal class ReadModelEntityFramework : IReadModelEntityFramework
    {
        [NotNull] private readonly IEagleEyeRepository repository;

        public ReadModelEntityFramework([NotNull] IEagleEyeRepository repository)
        {
            Guard.Argument(repository, nameof(repository)).NotNull();
            this.repository = repository;
        }

        public async Task<IEnumerable<Interface.Model.Photo>> GetAllPhotosAsync()
        {
            var result = await repository.GetAllAsync().ConfigureAwait(false);

            return result?.Select(MapPhoto).ToList();
        }

        public async Task<Interface.Model.Photo> GetPhotoByGuidAsync(Guid id)
        {
            var result = await repository.GetByIdAsync(id).ConfigureAwait(false);

            if (result == null)
                return null;

            return MapPhoto(result);
        }

        internal static Interface.Model.Photo MapPhoto([NotNull] Photo photo)
        {
            Guard.Argument(photo, nameof(photo)).NotNull();

            return new Interface.Model.Photo(
                photo.Id,
                photo.Filename,
                photo.FileMimeType,
                photo.FileSha256,
                photo.Tags?.Select(x => x.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly(),
                photo.People?.Select(x => x.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly(),
                MapLocation(photo.Location),
                photo.DateTimeTaken,
                photo.Version);
        }

        [CanBeNull]
        internal static Interface.Model.Location MapLocation([CanBeNull] Location location)
        {
            if (location == null)
                return null;

            return new Interface.Model.Location(
                location.CountryCode,
                location.CountryName,
                location.City,
                location.State,
                location.SubLocation);
        }
    }
}
