namespace EagleEye.Bootstrap.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Module;
    using FluentAssertions;
    using JetBrains.Annotations;
    using SimpleInjector;
    using Xunit;

    public class EagleEyeServicesTest
    {
        [NotNull] private readonly Container container;
        [NotNull] private readonly CallCounter callCounter;
        [NotNull] private readonly EagleEyeInitializeCallCounter eagleEyeInitializeCallCounter;

        public EagleEyeServicesTest()
        {
            container = new Container();
            callCounter = new CallCounter();
            container.RegisterInstance(callCounter);
            eagleEyeInitializeCallCounter = new EagleEyeInitializeCallCounter();
            container.RegisterInstance(eagleEyeInitializeCallCounter);
        }

        [Fact]
        public void InitializeServices_ShouldNotThrow_WhenContainerDoesNotContainServices()
        {
            // arrange
            var sut = new EagleEyeServices(container);

            // act
            Func<Task> act = async () => await sut.InitializeServices();

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public async Task InitializeServices_ShouldInitializeServices()
        {
            // arrange
            container.Collection.Append<IEagleEyeInitialize, DummyEagleEyeInitialize1>();
            var sut = new EagleEyeServices(container);

            // act
            await sut.InitializeServices();

            // assert
            eagleEyeInitializeCallCounter.Initialized.Should().BeEquivalentTo(nameof(DummyEagleEyeInitialize1));
        }

        [Fact]
        public async Task InitializeAllServices_ShouldNotThrow_WhenContainerDoesNotContainServices()
        {
            // arrange
            container.Collection.Append<IEagleEyeProcess, DummyEagleEyeProcess1>(Lifestyle.Transient);
            container.Collection.Append<IEagleEyeProcess, DummyEagleEyeProcess2>(Lifestyle.Transient);

            var sut = new EagleEyeServices(container);
            await sut.InitializeServices();

            // act
            sut.StartServices();

            // assert
            callCounter.Started.Count.Should().Be(2);
            callCounter.Stopped.Count.Should().Be(0);
            callCounter.Disposed.Count.Should().Be(0);
        }

        [Fact]
        public void StopServices_DoesNotDoAnything_WhenNotStarted()
        {
            // arrange
            var sut = new EagleEyeServices(container);

            // act
            Action act = () => sut.StopServices();

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public async Task StopServices_DoesNotDoAnything_WhenOnlyInitialized()
        {
            // arrange
            container.Collection.Append<IEagleEyeProcess, DummyEagleEyeProcess1>(Lifestyle.Transient);
            var sut = new EagleEyeServices(container);
            await sut.InitializeServices();

            // act
            Action act = () => sut.StopServices();

            // assert
            act.Should().NotThrow();
        }

        [Fact]
        public async Task StopServices_StopsStartedServices_WhenStarted()
        {
            // arrange
            container.Collection.Append<IEagleEyeProcess, DummyEagleEyeProcess1>(Lifestyle.Transient);
            var sut = new EagleEyeServices(container);
            await sut.InitializeServices();
            sut.StartServices();

            // assume
            callCounter.Stopped.Count.Should().Be(0);

            // act
            sut.StopServices();

            // assert
            callCounter.Stopped.Count.Should().Be(1);
        }

        [UsedImplicitly]
        private class DummyEagleEyeInitialize1 : IEagleEyeInitialize
        {
            private readonly EagleEyeInitializeCallCounter callCounter;
            private readonly string name = nameof(DummyEagleEyeInitialize1);

            public DummyEagleEyeInitialize1(EagleEyeInitializeCallCounter callCounter)
            {
                this.callCounter = callCounter;
            }

            public Task InitializeAsync()
            {
                callCounter.AddInitialized(name);
                return Task.CompletedTask;
            }
        }

        [UsedImplicitly]
        private class DummyEagleEyeProcess1 : IEagleEyeProcess
        {
            private readonly CallCounter cc;
            private readonly string name;

            public DummyEagleEyeProcess1(CallCounter cc)
            {
                name = nameof(DummyEagleEyeProcess1);
                this.cc = cc;
            }

            public void Dispose() => cc.AddDispose(name);

            public void Start() => cc.AddStart(name);

            public void Stop() => cc.AddStop(name);
        }

        [UsedImplicitly]
        private class DummyEagleEyeProcess2 : IEagleEyeProcess
        {
            private readonly CallCounter cc;
            private readonly string name;

            public DummyEagleEyeProcess2(CallCounter cc)
            {
                name = nameof(DummyEagleEyeProcess2);
                this.cc = cc;
            }

            public void Dispose() => cc.AddDispose(name);

            public void Start() => cc.AddStart(name);

            public void Stop() => cc.AddStop(name);
        }

        private class EagleEyeInitializeCallCounter
        {
            private readonly List<string> initializedList = new List<string>();
            private readonly object syncLockInitialized = new object();

            public IEnumerable<string> Initialized => initializedList.ToList();

            public void AddInitialized(string name)
            {
                lock (syncLockInitialized)
                {
                    initializedList.Add(name);
                }
            }
        }

        private class CallCounter
        {
            private readonly List<string> startedList;
            private readonly List<string> stoppedList;
            private readonly List<string> disposedList;
            private readonly object syncLockStarted = new object();
            private readonly object syncLockStopped = new object();
            private readonly object syncLockDisposed = new object();

            public CallCounter()
            {
                startedList = new List<string>();
                stoppedList = new List<string>();
                disposedList = new List<string>();
            }

            public List<string> Started => startedList.ToList();

            public List<string> Stopped => stoppedList.ToList();

            public List<string> Disposed => disposedList.ToList();

            public void AddDispose(string name)
            {
                lock (syncLockDisposed)
                {
                    disposedList.Add(name);
                }
            }

            public void AddStart(string name)
            {
                lock (syncLockStarted)
                {
                    startedList.Add(name);
                }
            }

            public void AddStop(string name)
            {
                lock (syncLockStopped)
                {
                    stoppedList.Add(name);
                }
            }
        }
    }
}
