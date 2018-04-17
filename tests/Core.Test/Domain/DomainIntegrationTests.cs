namespace EagleEye.Core.Test.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Routing;

    using EagleEye.Core.Domain;
    using EagleEye.Core.Domain.CommandHandlers;
    using EagleEye.Core.Domain.Commands;
    using EagleEye.Core.Domain.Events;

    using FluentAssertions;

    using Xunit;

    public class DomainIntegrationTests
    {
        [Fact]
        public async Task Handle_CreateMediaItemCommand_ShouldPublishEventTest()
        {
            // arrange
            var publisher = new Router();
            var respository = new Repository(new InMemoryEventStore(publisher));
            var session = new Session(respository);
            var handler = new MediaItemCommandHandlers(session);
            var events = new List<IEvent>();
            publisher.RegisterHandler<MediaItemCreated>((evt, ct) =>
                                                        {
                                                            events.Add(evt);
                                                            return Task.CompletedTask;
                                                        });
            publisher.RegisterHandler<TagsAddedToMediaItem>((evt, ct) =>
                                                        {
                                                            events.Add(evt);
                                                            return Task.CompletedTask;
                                                        });

            // act
            var guid = Guid.NewGuid();
            var command = new CreateMediaItemCommand(guid, "aap", new [] {"zoo", "holiday"}, null);
            await handler.Handle(command).ConfigureAwait(false);

            var addTagsCommand = new AddTagsToMediaItemCommand(guid, "summer", "holiday");
            await handler.Handle(addTagsCommand).ConfigureAwait(false);

            addTagsCommand = new AddTagsToMediaItemCommand(guid, "summer", "soccer");
            await handler.Handle(addTagsCommand).ConfigureAwait(false);

            var removeTagsCommand = new RemoveTagsFromMediaItemCommand(guid, "summer");
            await handler.Handle(removeTagsCommand).ConfigureAwait(false);

            // assert
            events.Should().HaveCount(3);
        }
    }
}