namespace CQRSlite.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Routing;
    using CQRSlite.Test.Events;
    using CQRSlite.Test.WriteModel;
    using CQRSlite.Test.WriteModel.Commands;
    using CQRSlite.Test.WriteModel.Handlers;

    using FluentAssertions;

    using Xunit;
    using Xunit.Categories;

    public class CqrsLiteFrameworkTest
    {
        private readonly Router router;
        private readonly ICommandSender commandSender;
        private readonly IEventPublisher eventPublisher;
        private readonly IHandlerRegistrar handlerRegister;
        private readonly IRepository repository;
        private readonly ISession session;

        public CqrsLiteFrameworkTest()
        {
            router = new Router();
            commandSender = router;
            eventPublisher = router;
            handlerRegister = router;
            repository = new Repository(new InMemoryEventStore(eventPublisher));
            session = new Session(repository);
        }

        [Fact]
        [Exploratory]
        public async Task Test()
        {
            // arrange
            var guid = Guid.NewGuid();
            var name = "monkey123!@#";
            var events = new List<InventoryItemCreated>();

            var handler = new InventoryCommandHandlers(session);
            handlerRegister.RegisterHandler<CreateInventoryItem>(handler.Handle);
            handlerRegister.RegisterHandler<InventoryItemCreated>((evt, ct) =>
                                                            {
                                                                events.Add(evt);
                                                                return Task.CompletedTask;
                                                            });

            // act
            await commandSender.Send(new CreateInventoryItem(guid, name)).ConfigureAwait(false);

            // assert
            events.Should().ContainSingle();
            events.Single().Id.Should().Be(guid);
            events.Single().Name.Should().Be(name);
            events.Single().TimeStamp.Should().NotBe(default);
        }
    }
}
