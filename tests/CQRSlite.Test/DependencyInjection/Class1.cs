namespace CQRSlite.Test.DependencyInjection
{
    using System;
    using System.Linq;
    using System.Reflection;
    using CQRSlite.Caching;
    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Events;
    using CQRSlite.Messages;
    using CQRSlite.Queries;
    using CQRSlite.Test.EventHandlers;
    using CQRSlite.Test.Events;
    using CQRSlite.Test.WriteModel;
    using SimpleInjector;
    using Xunit;

    public class Class1
    {
        private static readonly Assembly ThisAssembly = typeof(Class1).Assembly;

        [Fact]
        public void Abc()
        {
            var container = new Container();

            RegisterCqrsLite(container);

            container.Verify();
        }

        [Fact]
        public void Abcd()
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
//            container.Collection.Register(typeof(IHandler<>), ThisAssembly);
//            container.Collection.Register(typeof(ICancellableHandler<>), ThisAssembly);

//            container.Register(typeof(ICommandHandler<>), ThisAssembly, Lifestyle.Transient);
//            container.Register(typeof(ICancellableCommandHandler<>), ThisAssembly, Lifestyle.Transient);

            container.Collection.Register(typeof(IEventHandler<>), ThisAssembly);

            //            container.Register(typeof(IQueryHandler<,>), ThisAssembly, Lifestyle.Transient);
            //            container.Register(typeof(ICancellableQueryHandler<,>), ThisAssembly, Lifestyle.Transient);
        }
    }
}
