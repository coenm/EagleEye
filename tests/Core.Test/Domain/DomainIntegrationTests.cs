namespace EagleEye.Core.Test.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Routing;

    using EagleEye.Photo.Domain.CommandHandlers;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.Domain.EventStore;

    using FluentAssertions;

    using Xunit;

    public class DomainIntegrationTests
    {
        [Fact]
        public async Task Handle_CreateMediaItemCommand_ShouldPublishEventTest()
        {
            // arrange
            var publisher = new Router();
            var repository = new Repository(new InMemoryEventStore(publisher));
            var session = new Session(repository);
            var handler = new MediaItemCommandHandlers(session);
            var events = new List<IEvent>();
            publisher.RegisterHandler<PhotoCreated>((evt, ct) =>
                                                        {
                                                            events.Add(evt);
                                                            return Task.CompletedTask;
                                                        });
            publisher.RegisterHandler<TagsAddedToPhoto>((evt, ct) =>
                                                        {
                                                            events.Add(evt);
                                                            return Task.CompletedTask;
                                                        });

            // act
            var hash = new byte[32];
            var command = new CreatePhotoCommand("aap", hash, "image/jpeg", new[] { "zoo", "holiday" }, null);
            var guid = command.Id;
            await handler.Handle(command).ConfigureAwait(false);

            var addTagsCommand = new AddTagsToPhotoCommand(guid, "summer", "holiday");
            await handler.Handle(addTagsCommand).ConfigureAwait(false);

            addTagsCommand = new AddTagsToPhotoCommand(guid, "summer", "soccer");
            await handler.Handle(addTagsCommand).ConfigureAwait(false);

            var removeTagsCommand = new RemoveTagsFromPhotoCommand(guid, "summer");
            await handler.Handle(removeTagsCommand).ConfigureAwait(false);

            // assert
            events.Should().HaveCount(3);
        }
    }
}
