namespace EagleEye.FileImporter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Startup
    {
        /// <summary>
        /// Bootstrap the application by setting up the Dependency Container.
        /// </summary>
        /// <param name="connectionStrings">set of connection strings.</param>
        public static Container ConfigureContainer([NotNull] ConnectionStrings connectionStrings)
        {
            Guard.Argument(connectionStrings, nameof(connectionStrings)).NotNull();

            var plugins = EagleEye.Bootstrap.Bootstrapper.FindAvailablePlugins();

            var config = new Dictionary<string, object>();

            var bootstrapper = EagleEye.Bootstrap.Bootstrapper.Initialize(plugins, config, connectionStrings.FilenameEventStore);

            bootstrapper.RegisterPhotoDatabaseReadModel(connectionStrings.ConnectionStringPhotoDatabase);
            bootstrapper.RegisterSearchEngineReadModel(connectionStrings.LuceneDirectory);
            bootstrapper.RegisterSimilarityReadModel(connectionStrings.Similarity, connectionStrings.HangFire);
            return bootstrapper.Finalize();
        }

        public static string CreateSqlLiteFileConnectionString([NotNull] string fullFilename)
        {
            Guard.Argument(fullFilename, nameof(fullFilename)).NotNull().NotWhiteSpace();
            return $"Filename={fullFilename};";
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
