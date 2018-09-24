// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
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
        private readonly Router _router;
        private readonly ICommandSender _commandSender;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHandlerRegistrar _handlerRegister;
        private readonly IRepository _repository;
        private readonly ISession _session;

        public CqrsLiteFrameworkTest()
        {
            _router = new Router();
            _commandSender = _router;
            _eventPublisher = _router;
            _handlerRegister = _router;
            _repository = new Repository(new InMemoryEventStore(_eventPublisher));
            _session = new Session(_repository);
        }

        [Fact, Exploratory]
        public async Task Test()
        {
            // arrange
            var guid = Guid.NewGuid();
            var name = "monkey123!@#";
            var events = new List<InventoryItemCreated>();

            var handler = new InventoryCommandHandlers(_session);
            _handlerRegister.RegisterHandler<CreateInventoryItem>(handler.Handle);
            _handlerRegister.RegisterHandler<InventoryItemCreated>((evt, ct) =>
                                                            {
                                                                events.Add(evt);
                                                                return Task.CompletedTask;
                                                            });

            // act
            await _commandSender.Send(new CreateInventoryItem(guid, name)).ConfigureAwait(false);

            // assert
            events.Should().ContainSingle();
            events.Single().Id.Should().Be(guid);
            events.Single().Name.Should().Be(name);
            events.Single().TimeStamp.Should().NotBe(default(DateTimeOffset));
        }
    }
}
