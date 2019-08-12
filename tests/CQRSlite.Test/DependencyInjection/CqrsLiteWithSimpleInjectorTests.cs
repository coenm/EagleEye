namespace CQRSlite.Test.DependencyInjection
{
    using System;
    using System.Reflection;

    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Test.Events;
    using CQRSlite.Test.WriteModel;
    using SimpleInjector;
    using Xunit;

    public class CqrsLiteWithSimpleInjectorTests
    {
        private static readonly Assembly ThisAssembly = typeof(CqrsLiteWithSimpleInjectorTests).Assembly;

        [Fact]
        public void RegisterCqrsLiteStuff_ShouldBeCorrect()
        {
            // arrange
            var container = new Container();

            // act
            RegisterCqrsLite(container);

            // assert
            container.Verify();
        }

        [Fact]
        public void RegisteredEventHandler_ShouldBeUsed_WhenEventIsPublished()
        {
            var container = new Container();

            RegisterCqrsLite(container);
            RegisterEventHandler(container);

            container.Verify();

            var eventPublisher = container.GetInstance<IEventPublisher>();
            eventPublisher.Publish(new InventoryItemCreated(Guid.NewGuid(), "aap"));
        }

        private void RegisterCqrsLite(Container container)
        {
            container.Register<SimpleInjectorCqrsRouter>(() => new SimpleInjectorCqrsRouter(container), Lifestyle.Singleton);
            container.Register<ICommandSender>(container.GetInstance<SimpleInjectorCqrsRouter>, Lifestyle.Singleton);
            container.Register<IEventPublisher>(container.GetInstance<SimpleInjectorCqrsRouter>, Lifestyle.Singleton);

            container.RegisterSingleton<ICache, MemoryCache>();

            // Repository has two public constructors.
            container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton);
            container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            container.Register<ISession, Session>(Lifestyle.Singleton);
            container.Register<IEventStore, InMemoryEventStore>(Lifestyle.Singleton);
        }

        private static void RegisterEventHandler(Container container)
        {
            container.Collection.Register(typeof(IEventHandler<>), ThisAssembly);
        }
    }
}
