﻿namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    internal class PersonsAddedToPhotoEventHandler : ICancellableEventHandler<PersonsAddedToPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IPhotoIndex photoIndex;

        public PersonsAddedToPhotoEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.NotNull(photoIndex, nameof(photoIndex));
            this.photoIndex = photoIndex;
        }

        public async Task Handle(PersonsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));
            Guard.NotNull(message.Persons, nameof(message.Persons));

            var storedItem = photoIndex.Search(message.Id) as Photo;

            if (storedItem == null)
                return;

            storedItem.Version = message.Version;
            if (storedItem.Persons == null)
                storedItem.Persons = new List<string>();

            var newPersons = message.Persons.Distinct()
                .Where(item => !storedItem.Persons.Contains(item))
                .ToArray();

            storedItem.Persons.AddRange(newPersons);

            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
        }
    }
}