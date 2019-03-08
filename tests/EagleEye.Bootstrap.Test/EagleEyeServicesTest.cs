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

        public EagleEyeServicesTest()
        {
            container = new Container();
            callCounter = new CallCounter();
            container.RegisterInstance(callCounter);
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
            container.Collection.Append<IEagleEyeInitialize, >();
            var sut = new EagleEyeServices(container);

            // act
            await sut.InitializeServices();

            // assert
            act.Should().NotThrow();
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

        private class DummyEagleEyeInitialize1 : IEagleEyeInitialize
        {
            public DummyEagleEyeInitialize1()
            {

            }

            public Task InitializeAsync()
            {

                return Task.CompletedTask;
            }
        }

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
    }

    public class CallCounter
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
