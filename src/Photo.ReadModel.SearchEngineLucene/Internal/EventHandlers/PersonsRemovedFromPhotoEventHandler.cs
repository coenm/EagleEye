namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    internal class PersonsRemovedFromPhotoEventHandler : ICancellableEventHandler<PersonsRemovedFromPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IPhotoIndex photoIndex;

        public PersonsRemovedFromPhotoEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }

        public async Task Handle(PersonsRemovedFromPhoto message, CancellationToken token = new CancellationToken())
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(message.Persons, nameof(message.Persons)).NotNull();

            if (!(photoIndex.Search(message.Id) is Photo storedItem))
                return;

            storedItem.Version = message.Version;
            if (storedItem.Persons == null)
                return;

            if (!storedItem.Persons.Any(t => message.Persons.Contains(t)))
                return;

            storedItem.Persons.RemoveAll(t => message.Persons.Contains(t));
            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
        }
    }
}
