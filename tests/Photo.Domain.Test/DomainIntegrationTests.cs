namespace EagleEye.Photo.Domain.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Routing;
    using EagleEye.Core.DefaultImplementations.EventStore;
    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.Domain.Services;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class DomainIntegrationTests
    {
        [Fact]
        public async Task Handle_CreateMediaItemCommand_ShouldPublishEventTest()
        {
            // arrange
            var version = 0;
            var publisher = new Router();
            var repository = new Repository(new InMemoryEventStore(publisher));
            var session = new Session(repository);
            var uniqueFilenameService = A.Fake<IUniqueFilenameService>();
            var handler0 = new UpdateFileHashCommandHandler(session);
            var handler1 = new AddTagsToPhotoCommandHandler(session);
            var handler2 = new CreatePhotoCommandHandler(session, uniqueFilenameService);
            var handler3 = new AddTagsToPhotoCommandHandler(session);
            var handler4 = new RemoveTagsFromPhotoCommandHandler(session);
            var events = new List<IEvent>();
            publisher.RegisterHandler<PhotoCreated>((evt, ct) =>
                                                        {
                                                            version = evt.Version;
                                                            events.Add(evt);
                                                            return Task.CompletedTask;
                                                        });
            publisher.RegisterHandler<TagsAddedToPhoto>((evt, ct) =>
                                                        {
                                                            version = evt.Version;
                                                            events.Add(evt);
                                                            return Task.CompletedTask;
                                                        });

            // act
            var hash = new byte[32];
            var command = new CreatePhotoCommand("me.jpg", hash, "image/jpeg");
            var guid = command.Id;
            await handler2.Handle(command, default).ConfigureAwait(false);

            var addTagsCommand = new AddTagsToPhotoCommand(guid, version, "zoo", "holiday");
            await handler1.Handle(addTagsCommand, default).ConfigureAwait(false);

            addTagsCommand = new AddTagsToPhotoCommand(guid, version, "summer", "holiday");
            await handler1.Handle(addTagsCommand, default).ConfigureAwait(false);

            addTagsCommand = new AddTagsToPhotoCommand(guid, version, "summer", "soccer");
            await handler3.Handle(addTagsCommand, default).ConfigureAwait(false);

            var removeTagsCommand = new RemoveTagsFromPhotoCommand(guid, version, "summer");
            await handler4.Handle(removeTagsCommand, default).ConfigureAwait(false);

            // assert
            events.Should().HaveCount(4);
        }
    }
}
