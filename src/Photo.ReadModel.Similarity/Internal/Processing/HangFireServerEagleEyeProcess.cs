namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing
{
    using System;

    using EagleEye.Core.Interfaces;
    using EagleEye.Core.Interfaces.Module;

    using Hangfire;
    using JetBrains.Annotations;

    internal class HangFireServerEagleEyeProcess : IEagleEyeProcess, IDisposable
    {
        private readonly object syncLock = new object();
        [CanBeNull] private BackgroundJobServer backgroundJobServer;

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
