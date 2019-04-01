namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using Dawn;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    internal class PersonsAddedToPhotoEventHandler : ICancellableEventHandler<PersonsAddedToPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IPhotoIndex photoIndex;

        public PersonsAddedToPhotoEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }

        public async Task Handle(PersonsAddedToPhoto message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(message.Persons, nameof(message.Persons)).NotNull();

            if (!(photoIndex.Search(message.Id) is Photo storedItem))
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
