namespace EagleEye.FileStamper.Console
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.FileStamper.Console.Scenarios.FixAndUpdateImportImages;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Startup
    {
        /// <summary>
        /// Bootstrap the application by setting up the Dependency Container.
        /// </summary>
        /// <param name="config">Additional config for plugins.</param>
        public static Container ConfigureContainer([CanBeNull] Dictionary<string, object> config = null)
        {
            var plugins = EagleEye.Bootstrap.Bootstrapper.FindAvailablePlugins();

            config ??= new Dictionary<string, object>();

            string connectionStringFilenameEventStore = null;

            var bootstrapper = EagleEye.Bootstrap.Bootstrapper.Initialize(plugins, config, connectionStringFilenameEventStore);

            bootstrapper.AddRegistrations(c => c.Register<IUpdateImportImageCommandHandler, UpdateImportImageCommandHandler>());

            // bootstrapper.RegisterPhotoDatabaseReadModel(connectionStrings.ConnectionStringPhotoDatabase);
            // bootstrapper.RegisterSearchEngineReadModel(connectionStrings.LuceneDirectory);
            // bootstrapper.RegisterSimilarityReadModel(connectionStrings.Similarity, connectionStrings.HangFire);
            return bootstrapper.Finalize();
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        public static async Task InitializeAllServices([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();
            if (instancesToInitialize.Length == 0)
                return;

            await Task.WhenAll(instancesToInitialize.Select(instance => instance.InitializeAsync())).ConfigureAwait(false);
        }

        public static void StartServices([NotNull] Container container)
        {
            var allInstances = container.GetAllInstances<IEagleEyeProcess>();
            foreach (var eagleEyeProcess in allInstances)
            {
                eagleEyeProcess.Start();
            }
        }

        public static void StopServices([NotNull] Container container)
        {
            var allEagleEyeProcesses = container.GetAllInstances<IEagleEyeProcess>();
            foreach (var eagleEyeProcess in allEagleEyeProcesses)
            {
                eagleEyeProcess.Stop();
            }
        }
    }
}
