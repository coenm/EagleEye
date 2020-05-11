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

            string connectionStringHangFire = connectionStrings.HangFire;
            var userDir = GetUserDirectory();

            var connectionStringSimilarity = CreateSqlLiteFileConnectionString(CreateFullFilename("Similarity.db"));

            var plugins = EagleEye.Bootstrap.Bootstrapper.FindAvailablePlugins();

            var config = new Dictionary<string, object>();

            var bootstrapper = EagleEye.Bootstrap.Bootstrapper.Initialize(userDir, plugins, config, connectionStrings.FilenameEventStore);
            bootstrapper.RegisterPhotoDatabaseReadModel("InMemory a");
            bootstrapper.RegisterSearchEngineReadModel("InMemory a");
            bootstrapper.RegisterSimilarityReadModel(connectionStringSimilarity, "InMemory a");
            return bootstrapper.Finalize();
        }

        public static string CreateFullFilename([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();
            return Path.Combine(GetUserDirectory(), filename);
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

        private static string GetUserDirectory()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userDir, "EagleEye");
        }
    }
}
