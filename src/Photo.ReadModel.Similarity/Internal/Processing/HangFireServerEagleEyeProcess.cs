namespace Photo.ReadModel.Similarity.Internal.Processing
{
    using System;

    using EagleEye.Core.Interfaces;
    using Hangfire;
    using Hangfire.MemoryStorage;
    using Hangfire.SQLite;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Photo.ReadModel.Similarity.Internal.SimpleInjectorAdapter;
    using SimpleInjector;

    internal class HangFireServerEagleEyeProcess : IEagleEyeProcess, IDisposable
    {
        private readonly object syncLock = new object();
        [CanBeNull] private BackgroundJobServer backgroundJobServer;

        public HangFireServerEagleEyeProcess([NotNull] Container container, [NotNull] string connectionString)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            // TODO this is not very nice  :|
            if (connectionString.StartsWith("InMemory"))
            {
                Hangfire.GlobalConfiguration
                    .Configuration
                    .UseActivator(new SimpleInjectorJobActivator(container))
                    .UseMemoryStorage();
            }
            else
            {
                Hangfire.GlobalConfiguration
                    .Configuration
                    .UseSQLiteStorage(connectionString)
                    .UseActivator(new SimpleInjectorJobActivator(container));
            }
        }

        public void Start()
        {
            if (backgroundJobServer != null)
                return;

            lock (syncLock)
            {
                if (backgroundJobServer != null)
                    return;

                var backgroundJobServerOptions = new BackgroundJobServerOptions
                {
                    WorkerCount = 1,
                };
                backgroundJobServer = new BackgroundJobServer(backgroundJobServerOptions);
            }
        }

        public void Stop()
        {
            lock (syncLock)
            {
                backgroundJobServer?.SendStop();
            }
        }

        public void Dispose()
        {
            lock (syncLock)
            {
                backgroundJobServer?.Dispose();
                backgroundJobServer = null;
            }
        }
    }
}
