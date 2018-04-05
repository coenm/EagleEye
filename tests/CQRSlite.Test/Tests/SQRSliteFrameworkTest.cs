namespace CQRSlite.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

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

    public class SQRSliteFrameworkTest
    {
        [Fact, Exploratory]
        public async Task ExperimentSQRSliteFrameworkTest()
        {
            // arrange
            var publisher = new Router();
            var respository = new Repository(new InMemoryEventStore(publisher));
            var session = new Session(respository);
            var handler = new InventoryCommandHandlers(session);

            var events = new List<InventoryItemCreated>();
            publisher.RegisterHandler<InventoryItemCreated>((evt, ct) =>
                                                            {
                                                                events.Add(evt);

                                                                return Task.CompletedTask;
                                                            });

            // act
            var guid = Guid.NewGuid();
            await handler.Handle(new CreateInventoryItem(guid, "aap")).ConfigureAwait(false);

            // assert
            events.Should().HaveCount(1);
        }
    }
}
